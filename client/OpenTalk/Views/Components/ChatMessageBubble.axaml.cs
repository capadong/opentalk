using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using OpenTalk.ViewModels;

namespace OpenTalk.Views.Components;

public partial class ChatMessageBubble : UserControl
{
    public ChatMessageBubble()
    {
        InitializeComponent();
    }

    private void ImagePreview_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not ChatMessageItemViewModel vm)
        {
            return;
        }

        var source = vm.ImageBitmap;
        if (source is null)
        {
            return;
        }

        var owner = TopLevel.GetTopLevel(this) as Window;
        var preview = CreatePreviewWindow(source, vm.Content, owner);
        preview.Show();
        e.Handled = true;
    }

    private static Window CreatePreviewWindow(Bitmap source, string title, Window? owner)
    {
        var image = new Image
        {
            Source = source,
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var scroll = new ScrollViewer
        {
            Content = image
        };

        var root = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#CC000000")),
            Padding = new Thickness(24),
            Child = scroll
        };

        var window = new Window
        {
            Width = 980,
            Height = 760,
            MinWidth = 640,
            MinHeight = 480,
            Background = Brushes.Black,
            Content = root,
            Title = string.IsNullOrWhiteSpace(title) ? "图片预览" : title,
            WindowStartupLocation = owner is null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner
        };

        root.PointerPressed += (_, args) =>
        {
            if (args.Source == root)
            {
                window.Close();
                args.Handled = true;
            }
        };

        return window;
    }
}
