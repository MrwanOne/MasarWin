using System;
using System.Globalization;
using System.Windows.Data;

namespace Masar.UI.Converters;

public class SemesterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int semester)
        {
            // Our ComboBox has 0: Semester 1, 1: Semester 2, 2: Summer (Semester 3)
            return semester - 1;
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int selectedIndex)
        {
            return selectedIndex + 1;
        }
        return 1;
    }
}
