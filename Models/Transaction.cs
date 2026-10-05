using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace X_menu_app.Models
{
    public class Transaction : INotifyPropertyChanged
    {
        private Guid _id = Guid.NewGuid();
        private DateTime _date = DateTime.Now;
        private string _label = string.Empty;
        private decimal _amount;
        private bool _isExpense = true;
        private bool _isSelected;
        private string _category = string.Empty;
        private string? _attachmentPath;

        public Guid Id { get => _id; set { if (_id != value) { _id = value; OnPropertyChanged(); } } }
        public DateTime Date { get => _date; set { if (_date != value) { _date = value; OnPropertyChanged(); } } }
        public string Label { get => _label; set { if (_label != value) { _label = value; OnPropertyChanged(); } } }
        public decimal Amount { get => _amount; set { if (_amount != value) { _amount = value; OnPropertyChanged(); } } }
        public bool IsExpense { get => _isExpense; set { if (_isExpense != value) { _isExpense = value; OnPropertyChanged(); } } }
        public bool IsSelected { get => _isSelected; set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(); } } }
        public string Category { get => _category; set { if (_category != value) { _category = value; OnPropertyChanged(); } } }
        public string? AttachmentPath { get => _attachmentPath; set { if (_attachmentPath != value) { _attachmentPath = value; OnPropertyChanged(); } } }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
