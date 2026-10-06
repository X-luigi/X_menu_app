using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using X_menu_app.Models;
using X_menu_app.Repositories;

namespace X_menu_app.ViewModels
{
    public class WalletViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Transaction> _transactions = new ObservableCollection<Transaction>();
        private decimal _balance;
        private Transaction? _selected;
        private string _newLabel = string.Empty;
        private string _searchText = string.Empty;
        private bool _allSelected = false;
        private string _sortMode = "Date"; // kept for compatibility
        private string _selectedSortMode = "date_desc";

        public ObservableCollection<Transaction> Transactions { get => _transactions; set { _transactions = value; OnPropertyChanged(); } }
        public Transaction? Selected { get => _selected; set { _selected = value; OnPropertyChanged(); } }
        public decimal Balance { get => _balance; private set { _balance = value; OnPropertyChanged(); } }
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ApplyFilter(); } }
        public bool AllSelected { get => _allSelected; set { if (_allSelected != value) { _allSelected = value; OnPropertyChanged(); OnPropertyChanged(nameof(ToggleSelectAllLabel)); } } }
        public string SortMode { get => _sortMode; set { _sortMode = value; OnPropertyChanged(); ApplySort(); } }
        public string SelectedSortMode { get => _selectedSortMode; set { if (_selectedSortMode != value) { _selectedSortMode = value; OnPropertyChanged(); ApplySort(); } } }

        public string ToggleSelectAllLabel => AllSelected ? "Tout désélectionner" : "Tout sélectionner";

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }

        public WalletViewModel()
        {
            AddCommand = new RelayCommand(_ => Add(), _ => true);
            EditCommand = new RelayCommand(_ => Edit(), _ => CanEdit);
            DeleteCommand = new RelayCommand(_ => Delete(), _ => CanDelete());
            SaveCommand = new RelayCommand(_ => Save(), _ => true);
            ToggleSelectAllCommand = new RelayCommand(_ => ToggleSelectAll(!AllSelected), _ => Transactions.Count > 0);
            ApplyFilterCommand = new RelayCommand(_ => ApplyFilter(), _ => true);
            ApplySortCommand = new RelayCommand(_ => ApplySort(), _ => true);
            SetSortCommand = new RelayCommand(param => SetSort(param?.ToString()), _ => true);

            Calculator = new CalculatorViewModel();

            Load();
            Today = DateTime.Now;
        }

        private void AttachHandlers(ObservableCollection<Transaction>? list)
        {
            if (list == null) return;
            foreach (var t in list)
            {
                t.PropertyChanged += Transaction_PropertyChanged;
            }
        }

        private void DetachHandlers(ObservableCollection<Transaction>? list)
        {
            if (list == null) return;
            foreach (var t in list)
            {
                t.PropertyChanged -= Transaction_PropertyChanged;
            }
        }

        private void Transaction_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Transaction.IsSelected))
            {
                OnPropertyChanged(nameof(CanEdit));
                OnPropertyChanged(nameof(ToggleSelectAllLabel));
            }
        }

        private void Load()
        {
            var list = WalletRepository.Load();
            Transactions = new ObservableCollection<Transaction>(list);
            RecalculateBalance();
        }

        private void Save()
        {
            WalletRepository.Save(Transactions);
        }

        private void Add()
        {
            // Open AddTransactionWindow as modal dialog
            var t = new Transaction { Date = DateTime.Now, Label = "", Amount = 0.0m, IsExpense = true, Category = "Divers" };
            var win = new Views.AddTransactionWindow();
            var vm = new AddTransactionViewModel(t,
                onOk: () => { Transactions.Insert(0, t); Selected = t; RecalculateBalance(); Save(); },
                onCancel: () => { /* nothing */ });
            win.DataContext = vm;
            var owner = System.Windows.Application.Current?.MainWindow;
            if (owner != null) win.Owner = owner;
            win.ShowDialog();
        }

        public ICommand ToggleSelectAllCommand { get; }
        public ICommand ApplyFilterCommand { get; }
        public ICommand ApplySortCommand { get; }
        public ICommand SetSortCommand { get; }
        public CalculatorViewModel Calculator { get; }

        private bool CanDelete() => Transactions.Any(t => t.IsSelected) || Selected != null;

        private void ToggleSelectAll(bool select)
        {
            foreach (var t in Transactions)
                t.IsSelected = select;
            // update backing field and raise notifications without causing recursion
            _allSelected = select;
            OnPropertyChanged(nameof(AllSelected));
            OnPropertyChanged(nameof(ToggleSelectAllLabel));
            OnPropertyChanged(nameof(CanEdit));
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // reload from repository to remove filter
                Transactions = new ObservableCollection<Transaction>(WalletRepository.Load());
            }
            else
            {
                var filtered = WalletRepository.Load().Where(t =>
                    (t.Label != null && t.Label.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (t.Category != null && t.Category.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();
                Transactions = new ObservableCollection<Transaction>(filtered);
            }
            RecalculateBalance();
        }

        private void SetSort(string? mode)
        {
            if (!string.IsNullOrEmpty(mode))
            {
                SelectedSortMode = mode;
            }
        }

        private void ApplySort()
        {
            if (string.IsNullOrEmpty(SelectedSortMode))
                return;

            switch (SelectedSortMode)
            {
                case "date_desc":
                    Transactions = new ObservableCollection<Transaction>(Transactions.OrderByDescending(t => t.Date));
                    break;
                case "date_asc":
                    Transactions = new ObservableCollection<Transaction>(Transactions.OrderBy(t => t.Date));
                    break;
                case "amount_asc":
                    Transactions = new ObservableCollection<Transaction>(Transactions.OrderBy(t => t.Amount));
                    break;
                case "amount_desc":
                    Transactions = new ObservableCollection<Transaction>(Transactions.OrderByDescending(t => t.Amount));
                    break;
                case "deposit_first":
                    Transactions = new ObservableCollection<Transaction>(Transactions.OrderBy(t => t.IsExpense));
                    break;
                case "expense_first":
                    Transactions = new ObservableCollection<Transaction>(Transactions.OrderByDescending(t => t.IsExpense));
                    break;
                default:
                    // fallback: do nothing
                    break;
            }

            RecalculateBalance();
        }

        private void Edit()
        {
            // allow editing only when exactly one item is selected via checkbox
            int selectedCount = Transactions.Count(t => t.IsSelected);

            if (selectedCount == 0)
            {
                System.Windows.MessageBox.Show(
                    "⚠️ Veuillez sélectionner une transaction à modifier",
                    "Aucune sélection",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
                return;
            }

            if (selectedCount > 1)
            {
                System.Windows.MessageBox.Show(
                    "⚠️ Une seule transaction à la fois peut être modifiée.\nVeuillez désélectionner les autres transactions.",
                    "Plusieurs sélections",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            var toEdit = Transactions.FirstOrDefault(t => t.IsSelected);
            if (toEdit == null) return;

            // clone values to temp object, apply if OK
            var temp = new Transaction {
                Id = toEdit.Id,
                Date = toEdit.Date,
                Label = toEdit.Label,
                Amount = toEdit.Amount,
                IsExpense = toEdit.IsExpense,
                Category = toEdit.Category,
                AttachmentPath = toEdit.AttachmentPath
            };

            var win = new Views.AddTransactionWindow();
            var vm = new AddTransactionViewModel(temp,
                onOk: () => {
                    // copy back
                    toEdit.Date = temp.Date;
                    toEdit.Label = temp.Label;
                    toEdit.Amount = temp.Amount;
                    toEdit.IsExpense = temp.IsExpense;
                    toEdit.Category = temp.Category;
                    toEdit.AttachmentPath = temp.AttachmentPath;
                    Save();
                    RecalculateBalance();
                },
                onCancel: () => { });
            win.DataContext = vm;
            var owner = System.Windows.Application.Current?.MainWindow;
            if (owner != null) win.Owner = owner;
            win.ShowDialog();
            OnPropertyChanged(nameof(CanEdit));
        }

        private void Delete()
        {
            var toRemove = Transactions.Where(t => t.IsSelected).ToList();
            if (!toRemove.Any() && Selected != null) toRemove.Add(Selected);
            foreach (var t in toRemove)
            {
                t.PropertyChanged -= Transaction_PropertyChanged;
                Transactions.Remove(t);
            }
            Selected = null;
            RecalculateBalance();
            Save();
            OnPropertyChanged(nameof(CanEdit));
        }

        public bool CanEdit => Transactions.Count(t => t.IsSelected) == 1;

        private void RecalculateBalance()
        {
            Balance = Transactions.Sum(t => t.IsExpense ? -t.Amount : t.Amount);
        }

        public DateTime Today { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
