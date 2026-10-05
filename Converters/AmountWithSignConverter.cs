using System.Globalization;
using System.Windows.Data;

namespace X_menu_app.Converters
{
    public class AmountWithSignConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                // Utiliser le format de devise de la culture
                string formatted = amount.ToString("C", culture);

                // Si c'est négatif, le signe - est déjà présent
                // Si c'est positif, nous l'avons par le parameter IsExpense qui sera géré ailleurs
                return formatted;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
