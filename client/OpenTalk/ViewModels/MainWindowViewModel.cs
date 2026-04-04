using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenTalk.Models;
using OpenTalk.Services;

namespace OpenTalk.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IAsyncDisposable
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
        try
        {
            IsBusy = true;
            NotifyCanExecuteChanged();
            ConversationMode = ConversationMode.Group;
            StatusText = $"正在进入 {group.Name}…";

            if (_joinedGroupId.HasValue && _joinedGroupId.Value != group.Id)
            {
                await _chatHubClient.LeaveGroupAsync(_joinedGroupId.Value);
            }

            await _chatHubClient.JoinGroupAsync(group.Id);
            _joinedGroupId = group.Id;

            var members = await _apiClient.GetGroupMembersAsync(group.Id);
            var messages = (await _apiClient.GetMessagesAsync(group.Id)).Reverse().ToList();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SelectedPeer = null;
                Members.Clear();
                foreach (var member in members)
                {
                    Members.Add(member);
                }

                Messages.Clear();
                foreach (var message in messages)
                {
                    Messages.Add(ChatMessageItemViewModel.FromMessage(message, CurrentUserId, Members));
                }

                CurrentTitle = group.Name;
                UpdateSubtitle();
            });

            StatusText = $"已进入 {group.Name}";
        }
        catch (Exception ex)
        {
            StatusText = $"加载群组失败：{ex.Message}";
        }
        finally
        {
            IsBusy = false;
            NotifyCanExecuteChanged();
        }
    }

    private void UpdateSubtitle()
    {
        CurrentSubtitle = ConversationMode switch
        {
            ConversationMode.Direct when SelectedPeer is not null => $"私聊 · @{SelectedPeer.Username} · {Messages.Count} 条消息",
            _ => $"{Members.Count} 位成员 · {Messages.Count} 条消息"
        };
    }

    private void OnGroupMessageReceived(ChatMessage message)
    {
        if (ConversationMode != ConversationMode.Group || SelectedGroup?.Id != message.GroupId)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            Messages.Add(ChatMessageItemViewModel.FromMessage(message, CurrentUserId, Members));
            UpdateSubtitle();
            StatusText = "收到群消息";
        });
    }

    private void OnDirectMessageReceived(ChatMessage message)
    {
        if (SelectedPeer is null)
        {
            return;
        }

        var peerId = message.SenderId == CurrentUserId ? message.ReceiverId : message.SenderId;
        if (ConversationMode != ConversationMode.Direct || peerId != SelectedPeer.UserId)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            Messages.Add(ChatMessageItemViewModel.FromMessage(message, CurrentUserId, Members));
            UpdateSubtitle();
            StatusText = "收到私聊消息";
        });
    }

    private void HandleConnectionStateChanged(string state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ConnectionState = state;
            StatusText = state switch
            {
                "Connecting" => "正在连接服务…",
                "Connected" => "连接成功",
                "Reconnecting" => "连接中断，正在重连…",
                "Disconnected" => "连接已断开",
                _ => state
            };
            NotifyCanExecuteChanged();
        });
    }

    public async ValueTask DisposeAsync()
    {
        _chatHubClient.GroupMessageReceived -= OnGroupMessageReceived;
        _chatHubClient.DirectMessageReceived -= OnDirectMessageReceived;
        _chatHubClient.StateChanged -= HandleConnectionStateChanged;
        await _chatHubClient.DisposeAsync();
    }
}

public partial class ChatMessageItemViewModel : ObservableObject
{
    public long Id { get; init; }
    public long SenderId { get; init; }
    public string SenderName { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public bool IsSelf { get; init; }
    public bool IsOther => !IsSelf;
    public string TimeText => CreatedAt.ToLocalTime().ToString("MM-dd HH:mm");

    public static ChatMessageItemViewModel FromMessage(ChatMessage message, long selfId, IEnumerable<GroupMember> members)
    {
        var member = members.FirstOrDefault(x => x.UserId == message.SenderId)
            ?? members.FirstOrDefault(x => x.UserId == message.ReceiverId);

        var senderName = message.SenderId == selfId
            ? "我"
            : member?.DisplayName ?? $"用户 {message.SenderId}";

        return new ChatMessageItemViewModel
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = senderName,
            Content = message.Content,
            CreatedAt = message.CreatedAt,
            IsSelf = message.SenderId == selfId
        };
    }
}

