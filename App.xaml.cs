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

            // Ensure WPF bindings use the culture for formatting
            System.Windows.FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(System.Windows.FrameworkElement),
                new System.Windows.FrameworkPropertyMetadata(
                    System.Windows.Markup.XmlLanguage.GetLanguage(fr.IetfLanguageTag)));

            base.OnStartup(e);
        }
    }

}
