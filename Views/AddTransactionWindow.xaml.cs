using System.Windows;

namespace X_menu_app.Views
{
    public partial class AddTransactionWindow : Window
    {
        public AddTransactionWindow()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.AddTransactionViewModel vm)
            {
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
