using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace X_menu_app.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool isExpense))
                return Colors.Black;

            string paramStr = parameter?.ToString() ?? "";
            string[] colors = paramStr.Split(';');

            if (colors.Length != 2)
                return Colors.Black;

            // isExpense = true => Retrait (Red), false => Dépôt (Green)
            string hexColor = isExpense ? colors[0] : colors[1];

            try
            {
                return (Color)ColorConverter.ConvertFromString(hexColor);
            }
            catch
            {
                return Colors.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
