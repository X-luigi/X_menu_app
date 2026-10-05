using System.Windows;
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

        private void SortMenuButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle popup visibility
            SortPopup.IsOpen = !SortPopup.IsOpen;

            // Position popup relative to button
            if (SortPopup.IsOpen)
            {
                SortPopup.HorizontalOffset = SortMenuButton.ActualWidth - 10;
                SortPopup.VerticalOffset = SortMenuButton.ActualHeight + 2;
                SortPopup.PlacementTarget = SortMenuButton;
            }
        }
    }
}
