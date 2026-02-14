using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Masar.UI.Services;

/// <summary>
/// تطبيق خدمة Toast بدون مكتبات خارجية
/// Pure WPF Toast Service Implementation
/// </summary>
public class ToastService : IToastService
{
    private static Window? _toastWindow;
    private static readonly Queue<ToastMessage> _messageQueue = new();
    private static bool _isShowing = false;

    public void ShowSuccess(string message, string? title = null)
    {
        QueueToast(new ToastMessage
        {
            Message = message,
            Title = title ?? "نجاح",
            Type = ToastType.Success,
            BackgroundColor = "#4CAF50",
            Icon = "✅"
        });
    }

    public void ShowError(string message, string? title = null)
    {
        QueueToast(new ToastMessage
        {
            Message = message,
            Title = title ?? "خطأ",
            Type = ToastType.Error,
            BackgroundColor = "#F44336",
            Icon = "❌"
        });
    }

    public void ShowWarning(string message, string? title = null)
    {
        QueueToast(new ToastMessage
        {
            Message = message,
            Title = title ?? "تحذير",
            Type = ToastType.Warning,
            BackgroundColor = "#FF9800",
            Icon = "⚠️"
        });
    }

    public void ShowInfo(string message, string? title = null)
    {
        QueueToast(new ToastMessage
        {
            Message = message,
            Title = title ?? "معلومة",
            Type = ToastType.Info,
            BackgroundColor = "#2196F3",
            Icon = "ℹ️"
        });
    }

    private void QueueToast(ToastMessage toast)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            _messageQueue.Enqueue(toast);
            if (!_isShowing)
            {
                ShowNextToast();
            }
        });
    }

    private void ShowNextToast()
    {
        if (_messageQueue.Count == 0)
        {
            _isShowing = false;
            return;
        }

        _isShowing = true;
        var toast = _messageQueue.Dequeue();

        var mainWindow = System.Windows.Application.Current.MainWindow;
        if (mainWindow == null) return;

        // إنشاء نافذة Toast
        _toastWindow = new Window
        {
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = Brushes.Transparent,
            ShowInTaskbar = false,
            Topmost = true,
            Width = 350,
            Height = 80,
            Owner = mainWindow
        };

        // المحتوى
        var border = new Border
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(toast.BackgroundColor)),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(15),
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 10,
                Opacity = 0.3,
                ShadowDepth = 3
            }
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // الأيقونة
        var icon = new TextBlock
        {
            Text = toast.Icon,
            FontSize = 24,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(icon, 0);
        grid.Children.Add(icon);

        // النص
        var textStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        
        var titleBlock = new TextBlock
        {
            Text = toast.Title,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            FontSize = 14,
            FlowDirection = FlowDirection.RightToLeft
        };
        textStack.Children.Add(titleBlock);

        var messageBlock = new TextBlock
        {
            Text = toast.Message,
            Foreground = Brushes.White,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            FlowDirection = FlowDirection.RightToLeft
        };
        textStack.Children.Add(messageBlock);

        Grid.SetColumn(textStack, 1);
        grid.Children.Add(textStack);

        border.Child = grid;
        _toastWindow.Content = border;

        // الموقع - أعلى يمين الشاشة
        _toastWindow.Left = mainWindow.Left + mainWindow.Width - _toastWindow.Width - 20;
        _toastWindow.Top = mainWindow.Top + 20;

        // الشفافية للحركة
        _toastWindow.Opacity = 0;
        _toastWindow.Show();

        // حركة الظهور
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
        _toastWindow.BeginAnimation(Window.OpacityProperty, fadeIn);

        // مؤقت للإخفاء
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        timer.Tick += (s, e) =>
        {
            timer.Stop();
            HideToast();
        };
        timer.Start();
    }

    private void HideToast()
    {
        if (_toastWindow == null) return;

        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
        fadeOut.Completed += (s, e) =>
        {
            _toastWindow?.Close();
            _toastWindow = null;
            ShowNextToast();
        };
        _toastWindow.BeginAnimation(Window.OpacityProperty, fadeOut);
    }

    private class ToastMessage
    {
        public string Message { get; set; } = "";
        public string Title { get; set; } = "";
        public ToastType Type { get; set; }
        public string BackgroundColor { get; set; } = "#333";
        public string Icon { get; set; } = "";
    }

    private enum ToastType { Success, Error, Warning, Info }
}
