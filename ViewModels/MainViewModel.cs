using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using X_menu_app.Models;

namespace X_menu_app.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _currentView = "Home";
        public string CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public string UserName { get; set; } = "Luigi";
        public string TodayDate { get; set; } = DateTime.Now.ToString("dd MMMM yyyy");

        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>
        {
            new Product { Name = "Pâtes Spaghetti", Quantity = "500", Price = 1.20m, IsAvailable = true },
            new Product { Name = "Lardons", Quantity = "200", Price = 2.50m, IsAvailable = true },
            new Product { Name = "Œufs", Quantity = "6", Price = 2.10m, IsAvailable = true },
            new Product { Name = "Parmesan", Quantity = "100", Price = 2.80m, IsAvailable = true }
        };

        public decimal TotalBasketPrice => 8.60m;

        public ICommand NavigateToHomeCommand { get; }
        public ICommand NavigateToCourseCommand { get; }
        public ICommand ExitCommand { get; }

        public MainViewModel()
        {
            NavigateToHomeCommand = new RelayCommand(() => CurrentView = "Home");
            NavigateToCourseCommand = new RelayCommand(() => CurrentView = "Course");
            ExitCommand = new RelayCommand(() => System.Windows.Application.Current.Shutdown());
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
