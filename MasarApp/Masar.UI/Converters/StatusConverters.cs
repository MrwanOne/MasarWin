using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Masar.Domain.Enums;

namespace Masar.UI.Converters;

/// <summary>
/// يحول حالة المشروع إلى لون
/// Converts ProjectStatus to Color
/// </summary>
public class StatusToColorConverter : IValueConverter
{
    private static readonly Dictionary<ProjectStatus, string> StatusColors = new()
    {
        [ProjectStatus.Proposed] = "#FFA726",    // برتقالي - انتظار
        [ProjectStatus.Approved] = "#42A5F5",    // أزرق - موافقة
        [ProjectStatus.InProgress] = "#66BB6A",  // أخضر - تنفيذ
        [ProjectStatus.Completed] = "#26A69A",   // تركواز - مكتمل
        [ProjectStatus.Rejected] = "#EF5350"     // أحمر - مرفوض
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ProjectStatus status && StatusColors.TryGetValue(status, out var colorHex))
        {
            var color = (Color)ColorConverter.ConvertFromString(colorHex);
            return new SolidColorBrush(color);
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// يحول حالة المشروع إلى نص
/// Converts ProjectStatus to Display Text
/// </summary>
public class StatusToTextConverter : IValueConverter
{
    private static readonly Dictionary<ProjectStatus, string> StatusNamesAr = new()
    {
        [ProjectStatus.Proposed] = "مقترح",
        [ProjectStatus.Approved] = "معتمد",
        [ProjectStatus.InProgress] = "قيد التنفيذ",
        [ProjectStatus.Completed] = "مكتمل",
        [ProjectStatus.Rejected] = "مرفوض"
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ProjectStatus status && StatusNamesAr.TryGetValue(status, out var name))
        {
            return name;
        }
        return value?.ToString() ?? "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// يحول حالة المشروع إلى أيقونة
/// Converts ProjectStatus to Icon
/// </summary>
public class StatusToIconConverter : IValueConverter
{
    private static readonly Dictionary<ProjectStatus, string> StatusIcons = new()
    {
        [ProjectStatus.Proposed] = "⏳",    // ساعة رملية
        [ProjectStatus.Approved] = "✅",    // علامة صح
        [ProjectStatus.InProgress] = "🔄",  // سهم دائري
        [ProjectStatus.Completed] = "🎉",   // احتفال
        [ProjectStatus.Rejected] = "❌"     // علامة خطأ
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ProjectStatus status && StatusIcons.TryGetValue(status, out var icon))
        {
            return icon;
        }
        return "❓";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class FileSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return $"{dblSByte:0.##} {Suffix[i]}";
        }
        return "0 B";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
