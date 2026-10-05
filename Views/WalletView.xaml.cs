using System.Windows.Controls;

namespace X_menu_app.Views
{
    public partial class WalletView : UserControl
    {
        public WalletView()
        {
            // Delay setting DataContext to avoid issues when XAML view is missing
            InitializeComponent();
            DataContext = new ViewModels.WalletViewModel();
        }
    }
}
