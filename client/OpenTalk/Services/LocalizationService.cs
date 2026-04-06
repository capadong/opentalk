using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OpenTalk.Services;

public enum AppLanguage
{
    System = 0,
    ChineseSimplified = 1,
    English = 2,
}

public sealed class LocalizationService : ObservableObject
{
    private static readonly IReadOnlyDictionary<string, string> ChineseStrings = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["App.Name"] = "OpenTalk",
        ["Main.WindowTitle"] = "OpenTalk",
        ["Main.ConversationListSubtitle"] = "OpenTalk 会话列表",
        ["Main.SearchPlaceholder"] = "搜索",
        ["Main.MessagePlaceholder"] = "输入消息",
        ["Main.Send"] = "发送",
        ["Main.SelectFileTitle"] = "选择图片或文件",
        ["Main.InputHint"] = "Ctrl+Enter 发送，Enter 换行",
        ["Main.SelectGroupPrompt"] = "请选择一个群组开始聊天",
        ["Main.NoGroupsSubtitle"] = "当前账号没有可用群组",
        ["Main.NoGroupsStatus"] = "请先在服务端创建群组",
        ["Main.InitializingStatus"] = "正在连接服务…",
        ["Main.GroupMessageSent"] = "群消息已发送",
        ["Main.DirectMessageSent"] = "私聊消息已发送",
        ["Main.Uploading"] = "正在上传 {0}…",
        ["Main.UploadFailed"] = "上传失败",
        ["Main.ImageSent"] = "图片已发送",
        ["Main.FileSent"] = "文件已发送",
        ["Main.NoMoreHistory"] = "没有更多历史消息了",
        ["Main.HistoryLoaded"] = "已加载 {0} 条历史消息",
        ["Main.OpeningDirectChat"] = "正在打开与 {0} 的私聊…",
        ["Main.OpenedDirectChat"] = "已打开与 {0} 的私聊",
        ["Main.InitializationFailed"] = "初始化失败：{0}",
        ["Main.EnteringGroup"] = "正在进入群组：{0}…",
        ["Main.EnteredGroup"] = "已进入群组：{0}",
        ["Main.EnterGroupFailed"] = "进入群组失败：{0}",
        ["Main.GroupSubtitle"] = "{0} 位成员 · {1} 条消息",
        ["Main.DirectSubtitle"] = "{0} 条消息",
        ["Main.Connection.Connected"] = "已连接",
        ["Main.Connection.Connecting"] = "正在连接…",
        ["Main.Connection.Reconnecting"] = "正在重连…",
        ["Main.Connection.Disconnected"] = "已断开",
        ["Main.Connection.Other"] = "连接状态：{0}",
        ["Main.UserLabel"] = "用户 #{0}",
        ["Main.ConversationGroupSubtitle"] = "群聊 #{0}",
        ["Main.ConversationAvatarGroup"] = "群",
        ["Main.ConversationAvatarPerson"] = "人",
        ["Common.ImagePreview"] = "图片预览",
        ["Settings.WindowTitle"] = "设置 · OpenTalk",
        ["Settings.Header"] = "设置",
        ["Settings.Description"] = "在这里切换主题样式与界面语言（立即生效）",
        ["Settings.Theme"] = "主题",
        ["Settings.Language"] = "语言",
        ["Settings.Apply"] = "应用",
        ["Settings.CurrentSelection"] = "当前选择：{0}",
        ["Settings.Theme.System"] = "跟随系统",
        ["Settings.Theme.Light"] = "浅色",
        ["Settings.Theme.Dark"] = "深色",
        ["Settings.Language.System"] = "跟随系统",
        ["Settings.Language.ChineseSimplified"] = "简体中文",
        ["Settings.Language.English"] = "English",
    };

    private static readonly IReadOnlyDictionary<string, string> EnglishStrings = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["App.Name"] = "OpenTalk",
        ["Main.WindowTitle"] = "OpenTalk",
        ["Main.ConversationListSubtitle"] = "OpenTalk Conversations",
        ["Main.SearchPlaceholder"] = "Search",
        ["Main.MessagePlaceholder"] = "Type a message",
        ["Main.Send"] = "Send",
        ["Main.SelectFileTitle"] = "Choose an image or file",
        ["Main.InputHint"] = "Ctrl+Enter to send, Enter for a new line",
        ["Main.SelectGroupPrompt"] = "Select a group to start chatting",
        ["Main.NoGroupsSubtitle"] = "No groups are available for this account",
        ["Main.NoGroupsStatus"] = "Create a group on the server first",
        ["Main.InitializingStatus"] = "Connecting to the service…",
        ["Main.GroupMessageSent"] = "Group message sent",
        ["Main.DirectMessageSent"] = "Direct message sent",
        ["Main.Uploading"] = "Uploading {0}…",
        ["Main.UploadFailed"] = "Upload failed",
        ["Main.ImageSent"] = "Image sent",
        ["Main.FileSent"] = "File sent",
        ["Main.NoMoreHistory"] = "No more history messages",
        ["Main.HistoryLoaded"] = "Loaded {0} older messages",
        ["Main.OpeningDirectChat"] = "Opening a direct chat with {0}…",
        ["Main.OpenedDirectChat"] = "Opened a direct chat with {0}",
        ["Main.InitializationFailed"] = "Initialization failed: {0}",
        ["Main.EnteringGroup"] = "Joining group: {0}…",
        ["Main.EnteredGroup"] = "Joined group: {0}",
        ["Main.EnterGroupFailed"] = "Failed to join group: {0}",
        ["Main.GroupSubtitle"] = "{0} members · {1} messages",
        ["Main.DirectSubtitle"] = "{0} messages",
        ["Main.Connection.Connected"] = "Connected",
        ["Main.Connection.Connecting"] = "Connecting…",
        ["Main.Connection.Reconnecting"] = "Reconnecting…",
        ["Main.Connection.Disconnected"] = "Disconnected",
        ["Main.Connection.Other"] = "Connection state: {0}",
        ["Main.UserLabel"] = "User #{0}",
        ["Main.ConversationGroupSubtitle"] = "Group #{0}",
        ["Main.ConversationAvatarGroup"] = "G",
        ["Main.ConversationAvatarPerson"] = "P",
        ["Common.ImagePreview"] = "Image Preview",
        ["Settings.WindowTitle"] = "Settings · OpenTalk",
        ["Settings.Header"] = "Settings",
        ["Settings.Description"] = "Switch the theme and interface language here. Changes apply immediately.",
        ["Settings.Theme"] = "Theme",
        ["Settings.Language"] = "Language",
        ["Settings.Apply"] = "Apply",
        ["Settings.CurrentSelection"] = "Current selection: {0}",
        ["Settings.Theme.System"] = "System Default",
        ["Settings.Theme.Light"] = "Light",
        ["Settings.Theme.Dark"] = "Dark",
        ["Settings.Language.System"] = "System Default",
        ["Settings.Language.ChineseSimplified"] = "简体中文",
        ["Settings.Language.English"] = "English",
    };

    private readonly CultureInfo _systemCulture;

    private LocalizationService()
    {
        _systemCulture = CultureInfo.CurrentUICulture;
    }

    public static LocalizationService Instance { get; } = new();

    public event EventHandler? LanguageChanged;

    public AppLanguage CurrentLanguage { get; private set; } = AppLanguage.System;

    public CultureInfo CurrentCulture => ResolveCulture(CurrentLanguage);

    public string this[string key] => GetString(key);

    public void Apply(AppLanguage language)
    {
        CurrentLanguage = language;

        var culture = ResolveCulture(language);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        ApplyResources();

        OnPropertyChanged(nameof(CurrentLanguage));
        OnPropertyChanged(nameof(CurrentCulture));
        OnPropertyChanged("Item[]");

        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public string GetString(string key)
    {
        var strings = GetActiveStrings();
        if (strings.TryGetValue(key, out var value))
        {
            return value;
        }

        if (EnglishStrings.TryGetValue(key, out var fallback))
        {
            return fallback;
        }

        return key;
    }

    public string Format(string key, params object[] args)
    {
        return string.Format(CurrentCulture, GetString(key), args);
    }

    private CultureInfo ResolveCulture(AppLanguage language)
    {
        return language switch
        {
            AppLanguage.ChineseSimplified => new CultureInfo("zh-CN"),
            AppLanguage.English => new CultureInfo("en-US"),
            _ => _systemCulture,
        };
    }

    private IReadOnlyDictionary<string, string> GetActiveStrings()
    {
        var culture = ResolveCulture(CurrentLanguage);
        return culture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ? ChineseStrings : EnglishStrings;
    }

    private void ApplyResources()
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        foreach (var pair in GetActiveStrings())
        {
            app.Resources[pair.Key] = pair.Value;
        }
    }
}
