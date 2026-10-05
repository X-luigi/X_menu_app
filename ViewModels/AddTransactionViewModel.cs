using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using X_menu_app.Models;

namespace X_menu_app.ViewModels
{
    public class AddTransactionViewModel : INotifyPropertyChanged
    {
        public Transaction Transaction { get; }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public AddTransactionViewModel(Transaction t, Action onOk, Action onCancel)
        {
            Transaction = t;
            OkCommand = new RelayCommand(_ => onOk(), _ => true);
            CancelCommand = new RelayCommand(_ => onCancel(), _ => true);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
