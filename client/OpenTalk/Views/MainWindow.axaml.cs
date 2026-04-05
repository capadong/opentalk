using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using OpenTalk.ViewModels;

namespace OpenTalk.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
    }
}
