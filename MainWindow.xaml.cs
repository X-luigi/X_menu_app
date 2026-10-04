using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using X_menu_app.ViewModels;

namespace X_menu_app
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var culture = new CultureInfo("fr-FR");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void ThemeToggle_Checked(object sender, RoutedEventArgs e)
        {
            // switch to dark theme
            var dict = new ResourceDictionary { Source = new System.Uri("/Themes/DarkTheme.xaml", System.UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }

        private void ThemeToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            // switch to light theme
            var dict = new ResourceDictionary { Source = new System.Uri("/Themes/LightTheme.xaml", System.UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }
    }
}
