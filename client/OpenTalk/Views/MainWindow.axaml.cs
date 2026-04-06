using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System.Collections.Specialized;
using OpenTalk.ViewModels;

namespace OpenTalk.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? _subscribedVm;

        public MainWindow()
        {
            InitializeComponent();
            DataContextChanged += MainWindow_DataContextChanged;
        }

        private async void ChatScroll_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }

            // Near top => auto load history
            // Avalonia doesn't provide Offset on ScrollChangedEventArgs in some versions,
            // so we read current offset from the ScrollViewer itself.
            var sv = sender as ScrollViewer;
            if (sv is null)
            {
                return;
            }

            if (sv.Offset.Y <= 64 && vm.CanLoadMore)
            {
                // Capture current scroll metrics for "no-jump" adjustment.
                var oldExtent = sv.Extent.Height;

                await vm.LoadMoreCommand.ExecuteAsync(null);

                // If we prepended items, compensate scroll so content doesn't jump.
                if (vm.PendingPrependedCount > 0)
                {
                    // Let layout run once so Extent updates.
                    await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

                    var newExtent = sv.Extent.Height;
                    var delta = newExtent - oldExtent;
                    if (delta > 0)
                    {
                        sv.Offset = new Vector(sv.Offset.X, sv.Offset.Y + delta);
                    }

                    vm.PendingPrependedCount = 0;
                }
            }
        }

        private async void MessageInput_OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control) && DataContext is MainWindowViewModel vm)
            {
                e.Handled = true;
                await vm.SendCommand.ExecuteAsync(null);
            }
        }

        private async void PickFile_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }

            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "选择图片或文件",
                AllowMultiple = false
            });

            var file = files.Count > 0 ? files[0] : null;
            if (file is null)
            {
                return;
            }

            await vm.SendPickedFileAsync(file.Path.LocalPath);
        }

        private void MainWindow_DataContextChanged(object? sender, System.EventArgs e)
        {
            if (_subscribedVm is not null)
            {
                _subscribedVm.Messages.CollectionChanged -= Messages_CollectionChanged;
            }

            _subscribedVm = DataContext as MainWindowViewModel;
            if (_subscribedVm is not null)
            {
                _subscribedVm.Messages.CollectionChanged += Messages_CollectionChanged;
            }
        }

        private async void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }

            if (vm.PendingPrependedCount > 0 && e.NewStartingIndex == 0)
            {
                return;
            }

            if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Reset)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (this.FindControl<ScrollViewer>("ChatScroll") is { } sv)
                    {
                        sv.Offset = new Vector(sv.Offset.X, sv.Extent.Height);
                    }
                }, DispatcherPriority.Background);
            }
        }

        private void OpenSettings_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var w = new SettingsWindow
            {
                DataContext = new SettingsWindowViewModel(),
            };

            w.Show();
        }
    }
}
