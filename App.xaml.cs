using System.Configuration;
using System.Data;
using System.Windows;
using System.Globalization;

namespace X_menu_app
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Force French (France) culture to ensure numeric and currency formats use comma and € where appropriate
            var fr = new CultureInfo("fr-FR");
            CultureInfo.DefaultThreadCurrentCulture = fr;
            CultureInfo.DefaultThreadCurrentUICulture = fr;

            base.OnStartup(e);
        }
    }

}
