using Avalonia.Controls;
using Avalonia.Input;
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
    }
}
