using System.Globalization;
using System.Threading;
using System.Windows;
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
    }
}