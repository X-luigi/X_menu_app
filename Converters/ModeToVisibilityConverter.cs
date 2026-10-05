using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace X_menu_app.Converters
{
    public class ModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mode = value as string;
            var targetMode = parameter as string;

            return mode == targetMode ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
