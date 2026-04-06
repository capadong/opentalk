using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using OpenTalk.ViewModels;
using System;

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

    private async void MessageImage_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Height <= 0 || Math.Abs(e.NewSize.Height - e.PreviousSize.Height) < 1)
        {
            return;
        }

        if (this.FindAncestorOfType<ScrollViewer>() is not { } sv)
        {
            return;
        }

        var oldMaxY = Math.Max(0, sv.Extent.Height - sv.Viewport.Height);
        var wasNearBottom = Math.Abs(oldMaxY - sv.Offset.Y) <= 96;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            InvalidateMeasure();
            InvalidateArrange();
            sv.InvalidateMeasure();
            sv.InvalidateArrange();
            sv.UpdateLayout();

            if (!wasNearBottom)
            {
                return;
            }

            var newMaxY = Math.Max(0, sv.Extent.Height - sv.Viewport.Height);
            if (Math.Abs(newMaxY - sv.Offset.Y) > 1)
            {
                sv.Offset = new Vector(sv.Offset.X, newMaxY);
            }
        }, DispatcherPriority.Background);
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
