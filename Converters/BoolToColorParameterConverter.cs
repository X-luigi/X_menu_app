using System;
using System.Globalization;
using System.Windows.Data;

namespace X_menu_app.Converters
{
    public class BoolToColorParameterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool isExpense) || parameter == null)
                return "Black";

            var parts = parameter.ToString()!.Split('|');
            if (parts.Length != 2)
                return "Black";

            // isExpense = true => retrait (Red), isExpense = false => dépôt (Green)
            return isExpense ? parts[0] : parts[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
