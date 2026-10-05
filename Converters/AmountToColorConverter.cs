using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace X_menu_app.Converters
{
    public class AmountToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpense)
            {
                // Red for expense (retrait), Green for deposit (dépôt)
                return isExpense ? new SolidColorBrush(Color.FromRgb(217, 83, 79)) : new SolidColorBrush(Color.FromRgb(92, 184, 92));
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
