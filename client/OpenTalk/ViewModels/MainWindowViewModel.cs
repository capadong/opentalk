using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenTalk.Models;
using OpenTalk.Services;

namespace OpenTalk.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private const long DefaultUserId = 1;
    private const string DefaultApiBase = "https://localhost:59188";

    private readonly ApiClient _apiClient;
    private readonly ChatHubClient _chatHubClient;
    private long? _joinedGroupId;
    private readonly string _imageCacheDir;
    private readonly Dictionary<string, string> _imageCache = new(StringComparer.OrdinalIgnoreCase);
    private bool _suppressConversationSelectionChanged;

    public ObservableCollection<ChatGroup> Groups { get; } = [];
    public ObservableCollection<ConversationListEntry> Conversations { get; } = [];
    public ObservableCollection<ChatMessageItemViewModel> Messages { get; } = [];
    public ObservableCollection<GroupMember> Members { get; } = [];

    [ObservableProperty]
    private ChatGroup? selectedGroup;

    [ObservableProperty]
    private GroupMember? selectedPeer;

    [ObservableProperty]
    private ConversationListEntry? selectedConversation;

    [ObservableProperty]
    private ConversationMode conversationMode = ConversationMode.Group;

    [ObservableProperty]
    private string messageText = string.Empty;

    [ObservableProperty]
    private string connectionState = "Connecting";

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isLoadingHistory;

    [ObservableProperty]
    private string statusText = "正在连接服务…";

    [ObservableProperty]
    private string currentTitle = "OpenTalk";

    [ObservableProperty]
    private string currentSubtitle = "请选择一个群组开始聊天";

    [ObservableProperty]
    private string inputHint = "Ctrl+Enter 发送，Enter 换行";

    // Used for "no-jump" history loading
    [ObservableProperty]
    private int pendingPrependedCount;

    public long CurrentUserId => DefaultUserId;
    public string CurrentUserLabel => $"用户 #{CurrentUserId}";
    public bool IsGroupMode => ConversationMode == ConversationMode.Group;
    public bool IsDirectMode => ConversationMode == ConversationMode.Direct;
    public bool CanSend => !string.IsNullOrWhiteSpace(MessageText) && ConnectionState == "Connected" && (SelectedGroup is not null || SelectedPeer is not null);
    public bool CanLoadMore => !IsLoadingHistory && (SelectedGroup is not null || SelectedPeer is not null);

    public MainWindowViewModel()
    {
        _apiClient = new ApiClient(DefaultApiBase);
        _chatHubClient = new ChatHubClient(DefaultApiBase);
        _imageCacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenTalk", "image-cache");
        _chatHubClient.GroupMessageReceived += OnGroupMessageReceived;
        _chatHubClient.DirectMessageReceived += OnDirectMessageReceived;
        _chatHubClient.StateChanged += HandleConnectionStateChanged;

        SendCommand = new AsyncRelayCommand(SendAsync);
        LoadMoreCommand = new AsyncRelayCommand(LoadMoreAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        OpenGroupChatCommand = new AsyncRelayCommand(OpenGroupChatAsync);
        OpenDirectChatCommand = new AsyncRelayCommand<GroupMember?>(OpenDirectChatAsync);

        _ = InitializeAsync();
    }

    public IAsyncRelayCommand SendCommand { get; }
    public IAsyncRelayCommand LoadMoreCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand OpenGroupChatCommand { get; }
    public IAsyncRelayCommand<GroupMember?> OpenDirectChatCommand { get; }

    partial void OnSelectedGroupChanged(ChatGroup? value)
    {
        NotifyCanExecuteChanged();
    }

    partial void OnSelectedPeerChanged(GroupMember? value)
    {
        NotifyCanExecuteChanged();
    }

    partial void OnSelectedConversationChanged(ConversationListEntry? value)
    {
        if (_suppressConversationSelectionChanged || value is null)
        {
            return;
        }

        if (value.IsGroup)
        {
            var group = Groups.FirstOrDefault(g => g.Id == value.GroupId);
            if (group is null || (ConversationMode == ConversationMode.Group && SelectedGroup?.Id == group.Id))
            {
                return;
            }

            ConversationMode = ConversationMode.Group;
            SelectedPeer = null;
            SelectedGroup = group;
            _ = SelectGroupAsync(group);
            return;
        }

        var target = value.Member ?? Members.FirstOrDefault(m => m.UserId == value.PeerUserId);
        if (target is null || target.UserId == CurrentUserId || (ConversationMode == ConversationMode.Direct && SelectedPeer?.UserId == target.UserId))
        {
            return;
        }

        _ = OpenDirectChatAsync(target);
    }

    partial void OnConversationModeChanged(ConversationMode value)
    {
        NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(IsGroupMode));
        OnPropertyChanged(nameof(IsDirectMode));
    }

    partial void OnMessageTextChanged(string value)
    {
        NotifyCanExecuteChanged();
    }

    partial void OnConnectionStateChanged(string value)
    {
        NotifyCanExecuteChanged();
    }

    partial void OnIsLoadingHistoryChanged(bool value)
    {
        NotifyCanExecuteChanged();
    }

    private void NotifyCanExecuteChanged()
    {
        SendCommand.NotifyCanExecuteChanged();
        LoadMoreCommand.NotifyCanExecuteChanged();
        RefreshCommand.NotifyCanExecuteChanged();
        OpenGroupChatCommand.NotifyCanExecuteChanged();
        OpenDirectChatCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanSend));
        OnPropertyChanged(nameof(CanLoadMore));
    }

    private async Task SendAsync()
    {
        if (!CanSend)
        {
            return;
        }

        var content = MessageText.Trim();
        MessageText = string.Empty;

        if (ConversationMode == ConversationMode.Group && SelectedGroup is not null)
        {
            await _chatHubClient.SendMessageAsync(new SendMessageRequest
            {
                GroupId = SelectedGroup.Id,
                SenderId = CurrentUserId,
                Type = 1,
                Content = content
            });
            StatusText = "群消息已发送";
            return;
        }

        if (ConversationMode == ConversationMode.Direct && SelectedPeer is not null)
        {
            await _chatHubClient.SendDirectMessageAsync(new SendDirectMessageRequest
            {
                SenderId = CurrentUserId,
                ReceiverId = SelectedPeer.UserId,
                Type = 1,
                Content = content
            });
            StatusText = "私聊消息已发送";
        }
    }

    public async Task SendPickedFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || (!IsGroupMode && !IsDirectMode))
        {
            return;
        }

        StatusText = $"正在上传 {Path.GetFileName(filePath)}…";
        var upload = await _apiClient.UploadFileAsync(filePath, CurrentUserId);
        if (upload is null)
        {
            StatusText = "上传失败";
            return;
        }

        var messageType = IsImageFile(filePath) ? 2 : 3;
        if (ConversationMode == ConversationMode.Group && SelectedGroup is not null)
        {
            await _chatHubClient.SendMessageAsync(new SendMessageRequest
            {
                GroupId = SelectedGroup.Id,
                SenderId = CurrentUserId,
                Type = messageType,
                Content = upload.Name,
                FileUrl = upload.Url
            });
            StatusText = messageType == 2 ? "图片已发送" : "文件已发送";
            return;
        }

        if (ConversationMode == ConversationMode.Direct && SelectedPeer is not null)
        {
            await _chatHubClient.SendDirectMessageAsync(new SendDirectMessageRequest
            {
                SenderId = CurrentUserId,
                ReceiverId = SelectedPeer.UserId,
                Type = messageType,
                Content = upload.Name,
                FileUrl = upload.Url
            });
            StatusText = messageType == 2 ? "图片已发送" : "文件已发送";
        }
    }

    private static bool IsImageFile(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext is ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" or ".bmp";
    }

    private async Task LoadMoreAsync()
    {
        if (!CanLoadMore)
        {
            return;
        }

        IsLoadingHistory = true;
        try
        {
            var beforeId = Messages.FirstOrDefault()?.Id;
            List<ChatMessage> older;

            if (ConversationMode == ConversationMode.Direct && SelectedPeer is not null)
            {
                older = (await _apiClient.GetDirectMessagesAsync(CurrentUserId, SelectedPeer.UserId, 30, beforeId)).Reverse().ToList();
            }
            else if (SelectedGroup is not null)
            {
                older = (await _apiClient.GetMessagesAsync(SelectedGroup.Id, 30, beforeId)).Reverse().ToList();
            }
            else
            {
                return;
            }

            if (older.Count == 0)
            {
                StatusText = "没有更多历史消息了";
                return;
            }

            var olderItems = new List<ChatMessageItemViewModel>(older.Count);
            foreach (var msg in older)
            {
                olderItems.Add(await ToMessageItemAsync(msg));
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                PendingPrependedCount = olderItems.Count;
                for (var i = 0; i < olderItems.Count; i++)
                {
                    Messages.Insert(i, olderItems[i]);
                }
                UpdateSubtitle();
            });

            StatusText = $"已加载 {older.Count} 条历史消息";
        }
        finally
        {
            IsLoadingHistory = false;
        }
    }

    private Task RefreshAsync() => InitializeAsync();

    private async Task OpenGroupChatAsync()
    {
        if (SelectedGroup is null)
        {
            return;
        }

        ConversationMode = ConversationMode.Group;
        SelectedPeer = null;
        SelectConversationByGroupId(SelectedGroup.Id);
        await SelectGroupAsync(SelectedGroup);
    }

    private async Task OpenDirectChatAsync(GroupMember? member)
    {
        if (member is null || member.UserId == CurrentUserId)
        {
            return;
        }

        ConversationMode = ConversationMode.Direct;
        SelectedPeer = member;
        Messages.Clear();
        StatusText = $"正在打开与 {member.DisplayName} 的私聊…";
        EnsurePeerConversation(member);
        SelectConversationByPeerId(member.UserId);

        var messages = (await _apiClient.GetDirectMessagesAsync(CurrentUserId, member.UserId)).Reverse().ToList();
        foreach (var message in messages)
        {
            Messages.Add(await ToMessageItemAsync(message));
        }

        CurrentTitle = member.DisplayName;
        UpdateSubtitle();
        StatusText = $"已打开与 {member.DisplayName} 的私聊";
    }

    private async Task InitializeAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        NotifyCanExecuteChanged();
        try
        {
            await _chatHubClient.StartAsync(CurrentUserId);
            var groups = await _apiClient.GetGroupsAsync(CurrentUserId);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Groups.Clear();
                foreach (var group in groups)
                {
                    Groups.Add(group);
                }

                RebuildConversations();
            });

            if (groups.Count > 0)
            {
                SelectedGroup = groups[0];
                ConversationMode = ConversationMode.Group;
                SelectConversationByGroupId(groups[0].Id);
                await SelectGroupAsync(groups[0]);
            }
            else
            {
                CurrentSubtitle = "当前账号没有可用群组";
                StatusText = "请先在服务端创建群组";
            }
        }
        catch (Exception ex)
        {
            ConnectionState = "Disconnected";
            StatusText = $"初始化失败：{ex.Message}";
        }
        finally
        {
            IsBusy = false;
            NotifyCanExecuteChanged();
        }
    }

    private async Task SelectGroupAsync(ChatGroup group)
    {
        Messages.Clear();
        Members.Clear();

        try
        {
            StatusText = $"正在进入群组：{group.Name}…";

            if (_joinedGroupId.HasValue && _joinedGroupId.Value != group.Id)
            {
                await _chatHubClient.LeaveGroupAsync(_joinedGroupId.Value);
            }

            await _chatHubClient.JoinGroupAsync(group.Id);
            _joinedGroupId = group.Id;

            var members = await _apiClient.GetGroupMembersAsync(group.Id);
            foreach (var member in members
                         .Where(m => m.UserId != CurrentUserId)
                         .GroupBy(m => m.UserId)
                         .Select(g => g.First()))
            {
                Members.Add(member);
            }

            // Defensive: keep UI member list unique even if backend returns duplicates.
            DeduplicateMembers();
            RebuildConversations();
            SelectConversationByGroupId(group.Id);

            UpdateSubtitle();

            var recent = (await _apiClient.GetMessagesAsync(group.Id, 50)).Reverse().ToList();
            foreach (var message in recent)
            {
                Messages.Add(await ToMessageItemAsync(message));
            }

            CurrentTitle = group.Name;
            UpdateSubtitle();
            StatusText = $"已进入群组：{group.Name}";
        }
        catch (Exception ex)
        {
            StatusText = $"进入群组失败：{ex.Message}";
        }
    }

    private void DeduplicateMembers()
    {
        if (Members.Count <= 1)
        {
            return;
        }

        var seen = new HashSet<long>();
        for (var i = Members.Count - 1; i >= 0; i--)
        {
            var id = Members[i].UserId;
            if (!seen.Add(id))
            {
                Members.RemoveAt(i);
            }
        }
    }

    private void UpdateSubtitle()
    {
        var who = ConversationMode == ConversationMode.Group
            ? SelectedGroup?.Name
            : SelectedPeer?.DisplayName;

        if (string.IsNullOrWhiteSpace(who))
        {
            CurrentSubtitle = "";
            return;
        }

        var memberCount = Members.Count;
        CurrentSubtitle = ConversationMode == ConversationMode.Group
            ? $"{memberCount} 位成员 · {Messages.Count} 条消息"
            : $"{Messages.Count} 条消息";
    }

    private void HandleConnectionStateChanged(string state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ConnectionState = state;
            StatusText = state switch
            {
                "Connected" => "已连接",
                "Connecting" => "正在连接…",
                "Reconnecting" => "正在重连…",
                "Disconnected" => "已断开",
                _ => $"连接状态：{state}"
            };
        });
    }

    private void OnGroupMessageReceived(ChatMessage message)
    {
        if (ConversationMode != ConversationMode.Group)
        {
            return;
        }

        if (SelectedGroup is null || message.GroupId != SelectedGroup.Id)
        {
            return;
        }

        _ = AppendIncomingMessageAsync(message);
    }

    private void OnDirectMessageReceived(ChatMessage message)
    {
        if (ConversationMode != ConversationMode.Direct)
        {
            return;
        }

        if (SelectedPeer is null)
        {
            return;
        }

        var matches = (message.SenderId == SelectedPeer.UserId && message.ReceiverId == CurrentUserId)
            || (message.SenderId == CurrentUserId && message.ReceiverId == SelectedPeer.UserId);

        if (!matches)
        {
            return;
        }

        _ = AppendIncomingMessageAsync(message);
    }

    private async Task AppendIncomingMessageAsync(ChatMessage message)
    {
        var item = await ToMessageItemAsync(message);
        Dispatcher.UIThread.Post(() =>
        {
            Messages.Add(item);
            UpdateSubtitle();
        });
    }

    private async Task<ChatMessageItemViewModel> ToMessageItemAsync(ChatMessage message)
    {
        string? cachedImagePath = null;
        if (message.Type == 2 && !string.IsNullOrWhiteSpace(message.FileUrl))
        {
            cachedImagePath = await GetOrDownloadImageCachePathAsync(message.FileUrl);
        }

        return ChatMessageItemViewModel.FromMessage(message, CurrentUserId, Members, cachedImagePath);
    }

    private async Task<string?> GetOrDownloadImageCachePathAsync(string fileUrl)
    {
        if (_imageCache.TryGetValue(fileUrl, out var existing) && File.Exists(existing))
        {
            return existing;
        }

        try
        {
            var downloaded = await _apiClient.DownloadFileToCacheAsync(fileUrl, _imageCacheDir);
            if (!string.IsNullOrWhiteSpace(downloaded))
            {
                _imageCache[fileUrl] = downloaded;
            }

            return downloaded;
        }
        catch
        {
            return null;
        }
    }

    private void RebuildConversations()
    {
        Conversations.Clear();

        foreach (var group in Groups)
        {
            Conversations.Add(ConversationListEntry.FromGroup(group));
        }

        foreach (var member in Members
                     .Where(m => m.UserId != CurrentUserId)
                     .GroupBy(m => m.UserId)
                     .Select(g => g.First())
                     .OrderBy(m => m.DisplayName, StringComparer.OrdinalIgnoreCase))
        {
            Conversations.Add(ConversationListEntry.FromMember(member));
        }
    }

    private void EnsurePeerConversation(GroupMember member)
    {
        if (Conversations.Any(c => !c.IsGroup && c.PeerUserId == member.UserId))
        {
            return;
        }

        Conversations.Add(ConversationListEntry.FromMember(member));
    }

    private void SelectConversationByGroupId(long groupId)
    {
        var conversation = Conversations.FirstOrDefault(c => c.IsGroup && c.GroupId == groupId);
        SetSelectedConversationSilently(conversation);
    }

    private void SelectConversationByPeerId(long peerId)
    {
        var conversation = Conversations.FirstOrDefault(c => !c.IsGroup && c.PeerUserId == peerId);
        SetSelectedConversationSilently(conversation);
    }

    private void SetSelectedConversationSilently(ConversationListEntry? entry)
    {
        _suppressConversationSelectionChanged = true;
        SelectedConversation = entry;
        _suppressConversationSelectionChanged = false;
    }

    public sealed class ConversationListEntry
    {
        public bool IsGroup { get; init; }
        public long GroupId { get; init; }
        public long PeerUserId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Subtitle { get; init; } = string.Empty;
        public DateTime SortTime { get; init; }
        public GroupMember? Member { get; init; }
        public string AvatarText => IsGroup ? "群" : "人";
        public string TimeText => SortTime == default ? string.Empty : SortTime.ToString("MM/dd");

        public static ConversationListEntry FromGroup(ChatGroup group)
        {
            return new ConversationListEntry
            {
                IsGroup = true,
                GroupId = group.Id,
                Title = group.Name,
                Subtitle = $"群聊 #{group.Id}",
                SortTime = group.CreatedAt
            };
        }

        public static ConversationListEntry FromMember(GroupMember member)
        {
            return new ConversationListEntry
            {
                IsGroup = false,
                PeerUserId = member.UserId,
                Title = member.DisplayName,
                Subtitle = $"@{member.Username}",
                SortTime = member.JoinedAt,
                Member = member
            };
        }
    }
}
