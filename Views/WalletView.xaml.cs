using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace X_menu_app.Views
{
    public partial class WalletView : UserControl
    {
        public WalletView()
        {
            InitializeComponent();
            DataContext = new ViewModels.WalletViewModel();

            // Subscribe to DataContext property changes to monitor calculator mode
            this.DataContextChanged += (s, e) =>
            {
                if (this.DataContext is ViewModels.WalletViewModel vm)
                {
                    vm.Calculator.PropertyChanged += Calculator_PropertyChanged;
                }
            };
        }

        private void Calculator_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModels.CalculatorViewModel.Mode))
            {
                if (this.DataContext is ViewModels.WalletViewModel vm)
                {
                    switch (vm.Calculator.Mode)
                    {
                        case "calc":
                            CalcModePanel.Visibility = Visibility.Visible;
                            CurrencyModePanel.Visibility = Visibility.Collapsed;
                            DateModePanel.Visibility = Visibility.Collapsed;
                            break;
                        case "currency":
                            CalcModePanel.Visibility = Visibility.Collapsed;
                            CurrencyModePanel.Visibility = Visibility.Visible;
                            DateModePanel.Visibility = Visibility.Collapsed;
                            break;
                        case "date":
                            CalcModePanel.Visibility = Visibility.Collapsed;
                            CurrencyModePanel.Visibility = Visibility.Collapsed;
                            DateModePanel.Visibility = Visibility.Visible;
                            break;
                    }
                }
            }
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



        private void TransactionsList_Loaded(object sender, RoutedEventArgs e)
        {
            // Code à exécuter au chargement de la liste (peut rester vide)
        }
    }
}

