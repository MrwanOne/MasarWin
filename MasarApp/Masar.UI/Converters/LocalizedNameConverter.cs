using Masar.UI.Services;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace Masar.UI.Converters;

/// <summary>
/// يحول الكائن (CollegeDto / DepartmentDto) إلى الاسم المناسب حسب اللغة الحالية.
/// - اللغة عربية  → يعرض NameAr
/// - اللغة إنجليزية → يعرض NameEn
/// يستخدم مع ItemTemplate في ComboBox بدلاً من DisplayMemberPath.
/// </summary>
public class LocalizedNameConverter : IValueConverter, INotifyPropertyChanged
{
    private static ILocalizationService? _localizationService;

    public event PropertyChangedEventHandler? PropertyChanged;

    public static ILocalizationService? LocalizationService
    {
        get => _localizationService;
        set
        {
            if (_localizationService != null)
                _localizationService.LanguageChanged -= OnLanguageChanged;

            _localizationService = value;

            if (_localizationService != null)
                _localizationService.LanguageChanged += OnLanguageChanged;
        }
    }

    // عند تغيير اللغة، نخطر WPF بإعادة تقييم جميع الـ Bindings المرتبطة بهذا الـ Converter
    private static void OnLanguageChanged(object? sender, EventArgs e)
    {
        // نطلب من WPF تحديث كل ComboBox تستخدم هذا الـ Converter
        // عن طريق رفع PropertyChanged على الـ Instance المسجّل في App.xaml
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            // نعيد تحميل الـ ResourceDictionary لإجبار WPF على إعادة رسم الـ ItemTemplates
            // نجد الـ Converter المسجّل في App.Resources ونرسل إشعاراً
            if (System.Windows.Application.Current?.Resources["LocalizedNameConverter"]
                is LocalizedNameConverter converter)
            {
                converter.PropertyChanged?.Invoke(converter, new PropertyChangedEventArgs(nameof(LocalizationService)));
            }
        });
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;

        var isArabic = _localizationService?.IsArabic ?? true;

        // استخدام Reflection لدعم أي DTO يحتوي على NameAr / NameEn
        var type = value.GetType();
        var nameAr = type.GetProperty("NameAr")?.GetValue(value) as string ?? string.Empty;
        var nameEn = type.GetProperty("NameEn")?.GetValue(value) as string ?? string.Empty;

        if (isArabic)
            return string.IsNullOrWhiteSpace(nameAr) ? nameEn : nameAr;
        else
            return string.IsNullOrWhiteSpace(nameEn) ? nameAr : nameEn;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
