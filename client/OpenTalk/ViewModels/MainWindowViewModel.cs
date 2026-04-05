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

    public ObservableCollection<ChatGroup> Groups { get; } = [];
    public ObservableCollection<ChatMessageItemViewModel> Messages { get; } = [];
    public ObservableCollection<GroupMember> Members { get; } = [];

    [ObservableProperty]
    private ChatGroup? selectedGroup;

    [ObservableProperty]
    private GroupMember? selectedPeer;

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
        if (value is not null && ConversationMode == ConversationMode.Group)
        {
            _ = SelectGroupAsync(value);
        }
    }

    partial void OnSelectedPeerChanged(GroupMember? value)
    {
        NotifyCanExecuteChanged();
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

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                PendingPrependedCount = older.Count;
                for (var i = 0; i < older.Count; i++)
                {
                    Messages.Insert(i, ChatMessageItemViewModel.FromMessage(older[i], CurrentUserId, Members));
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

        var messages = (await _apiClient.GetDirectMessagesAsync(CurrentUserId, member.UserId)).Reverse().ToList();
        foreach (var message in messages)
        {
            Messages.Add(ChatMessageItemViewModel.FromMessage(message, CurrentUserId, Members));
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
            });

            if (groups.Count > 0)
            {
                SelectedGroup = groups[0];
                ConversationMode = ConversationMode.Group;
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

            UpdateSubtitle();

            var recent = (await _apiClient.GetMessagesAsync(group.Id, 50)).Reverse().ToList();
            foreach (var message in recent)
            {
                Messages.Add(ChatMessageItemViewModel.FromMessage(message, CurrentUserId, Members));
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

        Dispatcher.UIThread.Post(() =>
        {
            Messages.Add(ChatMessageItemViewModel.FromMessage(message, CurrentUserId, Members));
            UpdateSubtitle();
        });
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

        Dispatcher.UIThread.Post(() =>
        {
            Messages.Add(ChatMessageItemViewModel.FromMessage(message, CurrentUserId, Members));
            UpdateSubtitle();
        });
    }
}

