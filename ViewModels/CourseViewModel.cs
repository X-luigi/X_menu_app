using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows;
using System.IO;
using System.Windows.Threading;
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
        public ICommand EditSelectedCommand { get; set; }
        public ICommand SortByPriceAscCommand { get; set; }
        public ICommand SortByPriceDescCommand { get; set; }
        public ICommand SortByAvailableCommand { get; set; }
        public ICommand SortByUnavailableCommand { get; set; }
        public ICommand ToggleSelectAllCommand { get; set; }
        public string ToggleSelectAllLabel { get; set; } = "Tout sélectionner";
        public bool CanEdit => Products != null && Products.Count(p => p.IsSelected) == 1;
        private string? _selectedSortMode;
        private string? _selectedSortLabel;
        private string? _saveStatus;
        private DispatcherTimer? _saveIndicatorTimer;
        private bool _isSaveVisible;
        public string? SelectedSortMode
        {
            get => _selectedSortMode;
            set
            {
                _selectedSortMode = value;
                OnPropertyChanged();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    ApplySort(value);
                    SelectedSortLabel = GetLabelFromMode(value);
                }
            }
        }

        public string? SelectedSortLabel
        {
            get => _selectedSortLabel;
            set { _selectedSortLabel = value; OnPropertyChanged(); }
        }

        public string? SaveStatus
        {
            get => _saveStatus;
            set { _saveStatus = value; OnPropertyChanged(); }
        }

        public bool IsSaveVisible
        {
            get => _isSaveVisible;
            set { _isSaveVisible = value; OnPropertyChanged(); }
        }

        public CourseViewModel()
        {
            // Démarrer avec une liste vide : l'utilisateur remplira son catalogue
            Products = new ObservableCollection<Product>();

            foreach (var p in Products)
            {
                p.PropertyChanged += OnProductPropertyChanged;
            }

            // listen collection changes to update bindings (CanEdit, Toggle label, etc.)
            Products.CollectionChanged += Products_CollectionChanged;

            UpdateToggleLabel();

            FilteredProducts = CollectionViewSource.GetDefaultView(Products);
            FilteredProducts.Filter = FilterByName;

            // Load persisted products if available
            LoadProductsFromDisk();

            AddProductCommand = new RelayCommand(OpenAddProductDialog);
            DeleteSelectedCommand = new RelayCommand(DeleteSelected);
            SortCommand = new RelayCommand(SortProducts);
            EditSelectedCommand = new RelayCommand(EditSelected);
            SortByPriceAscCommand = new RelayCommand(() => ApplySort("price_asc"));
            SortByPriceDescCommand = new RelayCommand(() => ApplySort("price_desc"));
            SortByAvailableCommand = new RelayCommand(() => ApplySort("available"));
            SortByUnavailableCommand = new RelayCommand(() => ApplySort("unavailable"));
            ToggleSelectAllCommand = new RelayCommand(() => AllSelected = !AllSelected);

            // initialize sort selection (will call ApplySort)
            SelectedSortMode = "name_asc";

            // setup save indicator timer
            _saveIndicatorTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _saveIndicatorTimer.Tick += (s, e) => { SaveStatus = null; IsSaveVisible = false; _saveIndicatorTimer.Stop(); };
        }

        private void OnProductPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Product.IsSelected))
            {
                OnPropertyChanged(nameof(TotalPrice));
                UpdateToggleLabel();
                OnPropertyChanged(nameof(CanEdit));
            }
        }

        private void Products_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // ensure new items have handler and removed items detach
            if (e.NewItems != null)
            {
                foreach (Product p in e.NewItems)
                {
                    p.PropertyChanged += OnProductPropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (Product p in e.OldItems)
                {
                    p.PropertyChanged -= OnProductPropertyChanged;
                }
            }
            UpdateToggleLabel();
            OnPropertyChanged(nameof(CanEdit));
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
            UpdateToggleLabel();
        }

        private void UpdateToggleLabel()
        {
            if (Products != null && Products.Count > 0 && Products.All(p => p.IsSelected))
                ToggleSelectAllLabel = "Tout désélectionner";
            else
                ToggleSelectAllLabel = "Tout sélectionner";
            OnPropertyChanged(nameof(ToggleSelectAllLabel));
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
                // persist after adding
                SaveProductsToDisk();
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

            // After deletion update saved data
            SaveProductsToDisk();
        }

        private void EditSelected()
        {
            var sel = Products.FirstOrDefault(p => p.IsSelected);
            if (sel == null)
            {
                MessageBox.Show("Veuillez sélectionner un produit à modifier.", "Modifier", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // If more than one selected, disable edit
            var selectedCount = Products.Count(p => p.IsSelected);
            if (selectedCount != 1)
            {
                MessageBox.Show("Veuillez sélectionner exactement un produit pour modifier.", "Modifier", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Open AddProductWindow prefilled for editing
            var dlg = new Views.AddProductWindow();
            if (Application.Current?.MainWindow != null) dlg.Owner = Application.Current.MainWindow;

            // Prefill fields by setting textboxes via reflection of controls (since dialog exposes only getters currently)
            // Simpler approach: set initial values by showing dialog and then updating product if user confirms.
            // To prefill, we can add public methods/properties, but for now show message and reuse dialog blank.

            // Better: create an EditProductWindow that accepts a Product. For now, ask confirmation to modify basic fields.
            var res = MessageBox.Show($"Modifier '{sel.Name}' ?\n(La fenêtre d'édition s'ouvrira.)", "Modifier", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (res != MessageBoxResult.OK) return;

            // Open dialog and after confirmation, update properties
            var dialog = new Views.AddProductWindow();
            // Try to prefill controls if possible (use helper methods if implemented)
            if (Application.Current?.MainWindow != null) dialog.Owner = Application.Current.MainWindow;

            // Prefill dialog with existing product data
            dialog.Prefill(sel);

            // Show dialog
            if (dialog.ShowDialog() == true)
            {
                // Update product fields
                sel.Name = dialog.ProductName ?? sel.Name;
                sel.Quantity = dialog.ProductQuantity ?? sel.Quantity;
                sel.Price = dialog.ProductPrice;
                sel.IsAvailable = dialog.ProductIsAvailable;
                if (!string.IsNullOrWhiteSpace(dialog.ProductImagePath))
                {
                    // delete old images
                    try { if (!string.IsNullOrWhiteSpace(sel.ImagePath) && File.Exists(sel.ImagePath)) File.Delete(sel.ImagePath); } catch {}
                    try { if (!string.IsNullOrWhiteSpace(sel.OriginalImagePath) && File.Exists(sel.OriginalImagePath)) File.Delete(sel.OriginalImagePath); } catch {}

                    sel.ImagePath = dialog.ProductImagePath;
                    sel.OriginalImagePath = (dialog as Views.AddProductWindow)?.ProductOriginalImagePath;
                }
                OnPropertyChanged(nameof(TotalPrice));

                // Save changes
                SaveProductsToDisk();
            }
        }

        // Persistence: save/load product list (JSON) including image paths
        private string GetStoragePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "X_menu_app");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "products.json");
        }

        private void SaveProductsToDisk()
        {
            try
            {
                var path = GetStoragePath();
                var dto = Products.Select(p => new
                {
                    p.Name,
                    p.Quantity,
                    p.Price,
                    p.IsAvailable,
                    p.ImagePath,
                    p.OriginalImagePath
                }).ToList();
                var json = System.Text.Json.JsonSerializer.Serialize(dto);
                File.WriteAllText(path, json);
                // show save indicator
                SaveStatus = "Sauvegardé";
                IsSaveVisible = true;
                _saveIndicatorTimer?.Stop();
                _saveIndicatorTimer?.Start();
            }
            catch
            {
                // ignore persistence errors for now
            }
        }

        private void LoadProductsFromDisk()
        {
            try
            {
                var path = GetStoragePath();
                if (!File.Exists(path)) return;
                var json = File.ReadAllText(path);
                var list = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<ProductDto>>(json);
                if (list == null) return;
                foreach (var d in list)
                {
                    var prod = new Product
                    {
                        Name = d.Name,
                        Quantity = d.Quantity,
                        Price = d.Price,
                        IsAvailable = d.IsAvailable,
                        ImagePath = d.ImagePath,
                        OriginalImagePath = d.OriginalImagePath
                    };
                    prod.PropertyChanged += OnProductPropertyChanged;
                    Products.Add(prod);
                }
            }
            catch
            {
                // ignore
            }
        }

        private class ProductDto
        {
            public string Name { get; set; } = string.Empty;
            public string Quantity { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public bool IsAvailable { get; set; }
            public string? ImagePath { get; set; }
            public string? OriginalImagePath { get; set; }
        }

        private bool _sortAscending = true;
        private void SortProducts()
        {
            // default: toggle sort by Name
            FilteredProducts.SortDescriptions.Clear();
            var dir = _sortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending;
            FilteredProducts.SortDescriptions.Add(new SortDescription(nameof(Product.Name), dir));
            _sortAscending = !_sortAscending;
        }

        private void ApplySort(string mode)
        {
            FilteredProducts.SortDescriptions.Clear();
            switch (mode)
            {
                case "price_asc":
                    FilteredProducts.SortDescriptions.Add(new SortDescription(nameof(Product.Price), ListSortDirection.Ascending));
                    break;
                case "price_desc":
                    FilteredProducts.SortDescriptions.Add(new SortDescription(nameof(Product.Price), ListSortDirection.Descending));
                    break;
                case "name_asc":
                    FilteredProducts.SortDescriptions.Add(new SortDescription(nameof(Product.Name), ListSortDirection.Ascending));
                    break;
                case "available":
                    // Show available first
                    FilteredProducts.SortDescriptions.Add(new SortDescription(nameof(Product.IsAvailable), ListSortDirection.Descending));
                    break;
                case "unavailable":
                    // Show unavailable first
                    FilteredProducts.SortDescriptions.Add(new SortDescription(nameof(Product.IsAvailable), ListSortDirection.Ascending));
                    break;
                default:
                    break;
            }
            OnPropertyChanged(nameof(FilteredProducts));
        }

        private string GetLabelFromMode(string mode)
        {
            return mode switch
            {
                "price_asc" => "Prix (Croissant)",
                "price_desc" => "Prix (Décroissant)",
                "available" => "Etat : Disponible d'abord",
                "unavailable" => "Etat : Non-disponible d'abord",
                "name_asc" => "Nom (A-Z)",
                _ => "",
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}