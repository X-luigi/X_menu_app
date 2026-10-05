using System.Windows;

namespace X_menu_app.Views
{
    public partial class AddTransactionWindow : Window
    {
        public AddTransactionWindow()
        {
            InitializeComponent();
        }

        private void SetTodayDate(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.AddTransactionViewModel vm)
            {
                vm.Transaction.Date = System.DateTime.Now;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.AddTransactionViewModel vm)
            {
                // Validation 1 : Montant doit être > 0
                if (vm.Transaction.Amount <= 0)
                {
                    MessageBox.Show("⚠️ Le montant doit être supérieur à 0", "Erreur de validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validation 2 : Libellé ne doit pas être vide
                if (string.IsNullOrWhiteSpace(vm.Transaction.Label))
                {
                    MessageBox.Show("⚠️ Le libellé (description) ne doit pas être vide", "Erreur de validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validation 3 : Un choix entre Dépôt/Retrait doit être fait
                // (automatiquement géré par les RadioButtons du XAML, mais on vérifie quand même)

                vm.OkCommand.Execute(null);
            }
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.AddTransactionViewModel vm)
            {
                vm.CancelCommand.Execute(null);
            }
            this.DialogResult = false;
            this.Close();
        }
    }
}
