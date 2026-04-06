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
    private bool _noAvailableGroups;
    private string _statusKey = "Main.InitializingStatus";
    private object[] _statusArgs = [];

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
    private string statusText = LocalizationService.Instance.GetString("Main.InitializingStatus");

    [ObservableProperty]
    private string currentTitle = LocalizationService.Instance.GetString("App.Name");

    [ObservableProperty]
    private string currentSubtitle = LocalizationService.Instance.GetString("Main.SelectGroupPrompt");

    [ObservableProperty]
    private string inputHint = LocalizationService.Instance.GetString("Main.InputHint");

    // Used for "no-jump" history loading
    [ObservableProperty]
    private int pendingPrependedCount;

    public long CurrentUserId => DefaultUserId;
    public string CurrentUserLabel => LocalizationService.Instance.Format("Main.UserLabel", CurrentUserId);
    public string ConnectionStateLabel => GetConnectionStateLabel(ConnectionState);
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
        LocalizationService.Instance.LanguageChanged += LocalizationService_LanguageChanged;

        SendCommand = new AsyncRelayCommand(SendAsync);
        LoadMoreCommand = new AsyncRelayCommand(LoadMoreAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        OpenGroupChatCommand = new AsyncRelayCommand(OpenGroupChatAsync);
        OpenDirectChatCommand = new AsyncRelayCommand<GroupMember?>(OpenDirectChatAsync);

        RefreshLocalizedContent();
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
        OnPropertyChanged(nameof(ConnectionStateLabel));
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
            SetStatus("Main.GroupMessageSent");
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
            SetStatus("Main.DirectMessageSent");
        }
    }

    public async Task SendPickedFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || (!IsGroupMode && !IsDirectMode))
        {
            return;
        }

        SetStatus("Main.Uploading", Path.GetFileName(filePath));
        var upload = await _apiClient.UploadFileAsync(filePath, CurrentUserId);
        if (upload is null)
        {
            SetStatus("Main.UploadFailed");
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
            SetStatus(messageType == 2 ? "Main.ImageSent" : "Main.FileSent");
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
            SetStatus(messageType == 2 ? "Main.ImageSent" : "Main.FileSent");
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
                SetStatus("Main.NoMoreHistory");
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

            SetStatus("Main.HistoryLoaded", older.Count);
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
        SetStatus("Main.OpeningDirectChat", member.DisplayName);
        EnsurePeerConversation(member);
        SelectConversationByPeerId(member.UserId);

        var messages = (await _apiClient.GetDirectMessagesAsync(CurrentUserId, member.UserId)).Reverse().ToList();
        foreach (var message in messages)
        {
            Messages.Add(await ToMessageItemAsync(message));
        }

        CurrentTitle = member.DisplayName;
        UpdateSubtitle();
        SetStatus("Main.OpenedDirectChat", member.DisplayName);
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
                _noAvailableGroups = false;
                SelectedGroup = groups[0];
                ConversationMode = ConversationMode.Group;
                SelectConversationByGroupId(groups[0].Id);
                await SelectGroupAsync(groups[0]);
            }
            else
            {
                _noAvailableGroups = true;
                CurrentSubtitle = LocalizationService.Instance.GetString("Main.NoGroupsSubtitle");
                SetStatus("Main.NoGroupsStatus");
            }
        }
        catch (Exception ex)
        {
            ConnectionState = "Disconnected";
            SetStatus("Main.InitializationFailed", ex.Message);
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
            SetStatus("Main.EnteringGroup", group.Name);

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
            SetStatus("Main.EnteredGroup", group.Name);
        }
        catch (Exception ex)
        {
            SetStatus("Main.EnterGroupFailed", ex.Message);
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
            ? LocalizationService.Instance.Format("Main.GroupSubtitle", memberCount, Messages.Count)
            : LocalizationService.Instance.Format("Main.DirectSubtitle", Messages.Count);
    }

    private void HandleConnectionStateChanged(string state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ConnectionState = state;
            SetStatus(GetConnectionStatusKey(state), state);
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

    private void RebuildConversations(bool preserveSelection = false)
    {
        var selectedGroupId = preserveSelection ? SelectedGroup?.Id : null;
        var selectedPeerId = preserveSelection ? SelectedPeer?.UserId : null;

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

        if (selectedGroupId.HasValue)
        {
            SelectConversationByGroupId(selectedGroupId.Value);
        }
        else if (selectedPeerId.HasValue)
        {
            SelectConversationByPeerId(selectedPeerId.Value);
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
        public string AvatarText => IsGroup
            ? LocalizationService.Instance.GetString("Main.ConversationAvatarGroup")
            : LocalizationService.Instance.GetString("Main.ConversationAvatarPerson");
        public string TimeText => SortTime == default ? string.Empty : SortTime.ToString("MM/dd");

        public static ConversationListEntry FromGroup(ChatGroup group)
        {
            return new ConversationListEntry
            {
                IsGroup = true,
                GroupId = group.Id,
                Title = group.Name,
                Subtitle = LocalizationService.Instance.Format("Main.ConversationGroupSubtitle", group.Id),
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

    private void LocalizationService_LanguageChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(RefreshLocalizedContent);
    }

    private void RefreshLocalizedContent()
    {
        InputHint = LocalizationService.Instance.GetString("Main.InputHint");
        CurrentTitle = SelectedPeer?.DisplayName ?? SelectedGroup?.Name ?? LocalizationService.Instance.GetString("App.Name");
        CurrentSubtitle = SelectedPeer is not null || SelectedGroup is not null
            ? CurrentSubtitle
            : LocalizationService.Instance.GetString(_noAvailableGroups ? "Main.NoGroupsSubtitle" : "Main.SelectGroupPrompt");

        OnPropertyChanged(nameof(CurrentUserLabel));
        OnPropertyChanged(nameof(ConnectionStateLabel));
        RebuildConversations(preserveSelection: true);

        if (SelectedPeer is not null || SelectedGroup is not null)
        {
            UpdateSubtitle();
        }

        ApplyStatusText();
    }

    private void SetStatus(string key, params object[] args)
    {
        _statusKey = key;
        _statusArgs = args;
        ApplyStatusText();
    }

    private void ApplyStatusText()
    {
        StatusText = LocalizationService.Instance.Format(_statusKey, _statusArgs);
    }

    private static string GetConnectionStatusKey(string state)
    {
        return state switch
        {
            "Connected" => "Main.Connection.Connected",
            "Connecting" => "Main.Connection.Connecting",
            "Reconnecting" => "Main.Connection.Reconnecting",
            "Disconnected" => "Main.Connection.Disconnected",
            _ => "Main.Connection.Other",
        };
    }

    private static string GetConnectionStateLabel(string state)
    {
        return state switch
        {
            "Connected" => LocalizationService.Instance.GetString("Main.Connection.Connected"),
            "Connecting" => LocalizationService.Instance.GetString("Main.Connection.Connecting"),
            "Reconnecting" => LocalizationService.Instance.GetString("Main.Connection.Reconnecting"),
            "Disconnected" => LocalizationService.Instance.GetString("Main.Connection.Disconnected"),
            _ => LocalizationService.Instance.Format("Main.Connection.Other", state),
        };
    }
}
