using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows;
using System.IO;
using X_menu_app.Models;

namespace X_menu_app.ViewModels
{
    public class CourseViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Product> Products { get; set; }
        public ICollectionView FilteredProducts { get; set; }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilteredProducts?.Refresh();
            }
        }

        private bool _allSelected;
        public bool AllSelected
        {
            get => _allSelected;
            set
            {
                _allSelected = value;
                OnPropertyChanged();
                ToggleSelectAll(value);
            }
        }

        public decimal TotalPrice => Products.Where(p => p.IsSelected).Sum(p => p.Price);

        public ICommand AddProductCommand { get; set; }
        public ICommand DeleteSelectedCommand { get; set; }
        public ICommand SortCommand { get; set; }
        public ICommand ToggleSelectAllCommand { get; set; }

        public CourseViewModel()
        {
            // Données d'exemple simples sans besoin de dossier Assets
            Products = new ObservableCollection<Product>
            {
                new Product { Name = "Pack d'eau", Quantity = 6, Price = 1.52m, IsAvailable = true },
                new Product { Name = "Carton d'oeufs", Quantity = 30, Price = 7.32m, IsAvailable = false },
                new Product { Name = "Pâtes Spaghetti", Quantity = 500, Price = 1.20m, IsAvailable = true },
                new Product { Name = "Lardons", Quantity = 200, Price = 2.50m, IsAvailable = true },
                new Product { Name = "Parmesan", Quantity = 100, Price = 2.80m, IsAvailable = true }
            };

            foreach (var p in Products)
            {
                p.PropertyChanged += OnProductPropertyChanged;
            }

            FilteredProducts = CollectionViewSource.GetDefaultView(Products);
            FilteredProducts.Filter = FilterByName;

            AddProductCommand = new RelayCommand(OpenAddProductDialog);
            DeleteSelectedCommand = new RelayCommand(DeleteSelected);
            SortCommand = new RelayCommand(SortProducts);
            ToggleSelectAllCommand = new RelayCommand(() => AllSelected = !AllSelected);
        }

        private void OnProductPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Product.IsSelected))
            {
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        private bool FilterByName(object obj)
        {
            if (obj is Product p)
            {
                if (string.IsNullOrWhiteSpace(SearchText)) return true;
                return p.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            return false;
        }

        private void ToggleSelectAll(bool select)
        {
            foreach (var product in Products)
            {
                product.IsSelected = select;
            }
            OnPropertyChanged(nameof(TotalPrice));
        }

        private void OpenAddProductDialog()
        {
            // Open a dialog to collect product information
            var dlg = new Views.AddProductWindow();
            // Set owner if possible
            if (System.Windows.Application.Current?.MainWindow != null)
            {
                dlg.Owner = System.Windows.Application.Current.MainWindow;
            }

            var result = dlg.ShowDialog();
            if (result == true)
            {
                // Retrieve values from dialog
                var name = dlg.ProductName ?? "Nouveau Produit";
                var quantity = dlg.ProductQuantity;
                var price = dlg.ProductPrice;
                var isAvailable = dlg.ProductIsAvailable;
                var imagePath = dlg.ProductImagePath;

                var newProd = new Product
                {
                    Name = name,
                    Quantity = quantity,
                    Price = price,
                    IsAvailable = isAvailable,
                    ImagePath = imagePath,
                    OriginalImagePath = (dlg as Views.AddProductWindow)?.ProductOriginalImagePath
                };
                newProd.PropertyChanged += OnProductPropertyChanged;
                Products.Add(newProd);
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        private void DeleteSelected()
        {
            var selected = Products.Where(p => p.IsSelected).ToList();
            if (!selected.Any()) return;

            var msg = selected.Count == 1 ? "Supprimer l'article sélectionné ?" : $"Supprimer les {selected.Count} articles sélectionnés ?";
            var res = MessageBox.Show(msg, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;

            foreach (var prod in selected)
            {
                try
                {
                    // detach listeners
                    prod.PropertyChanged -= OnProductPropertyChanged;

                    // delete thumbnail and original image if they exist
                    if (!string.IsNullOrWhiteSpace(prod.ImagePath) && File.Exists(prod.ImagePath))
                    {
                        try { File.Delete(prod.ImagePath); } catch { /* ignore */ }
                    }
                    if (!string.IsNullOrWhiteSpace(prod.OriginalImagePath) && File.Exists(prod.OriginalImagePath))
                    {
                        try { File.Delete(prod.OriginalImagePath); } catch { /* ignore */ }
                    }
                }
                catch
                {
                    // ignore errors during cleanup
                }
                Products.Remove(prod);
            }
            OnPropertyChanged(nameof(TotalPrice));
        }

        private bool _sortAscending = true;
        private void SortProducts()
        {
            FilteredProducts.SortDescriptions.Clear();
            var dir = _sortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending;
            FilteredProducts.SortDescriptions.Add(new SortDescription(nameof(Product.Name), dir));
            _sortAscending = !_sortAscending;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}