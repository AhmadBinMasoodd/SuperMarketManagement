using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels.Base;
using SuperMarketManagement.Views.Admin;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SuperMarketManagement.ViewModels
{
    public class AdminDashboardViewModel : ViewModelBase
    {
        private readonly User _currentUser;

        private UserControl? _currentView;
        public UserControl? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        private string _activeMenu = string.Empty;
        public string ActiveMenu
        {
            get => _activeMenu;
            set => SetProperty(ref _activeMenu, value);
        }

        public ICommand NavigateDashboardCommand { get; }
        public ICommand NavigateEmployeesCommand { get; }
        public ICommand NavigateCategoryCommand { get; }
        public ICommand NavigateProductCommand { get; }
        public ICommand NavigateStockHistoryCommand { get; }
        public ICommand LogoutCommand { get; }

        public AdminDashboardViewModel(User user)
        {
            _currentUser = user;

            NavigateDashboardCommand = new RelayCommand(_ => ExecuteNavigateDashboard());
            NavigateEmployeesCommand = new RelayCommand(_ => ExecuteNavigateEmployees());
            NavigateCategoryCommand = new RelayCommand(_ => ExecuteNavigateCategory());
            NavigateProductCommand = new RelayCommand(_ => ExecuteNavigateProduct());
            NavigateStockHistoryCommand = new RelayCommand(_ => ExecuteNavigateStockHistory());
            LogoutCommand = new RelayCommand(_ => ExecuteLogout());

            // Default view
            ExecuteNavigateDashboard();
        }

        private void ExecuteNavigateDashboard()
        {
            CurrentView = new ChartOverview();
            ActiveMenu = "Dashboard";
        }

        private void ExecuteNavigateEmployees()
        {
            CurrentView = new Employee();
            ActiveMenu = "Employees";
        }

        private void ExecuteNavigateCategory()
        {
            CurrentView = new Views.Admin.Category();
            ActiveMenu = "Category";
        }

        private void ExecuteNavigateProduct()
        {
            CurrentView = new Views.Admin.Product(_currentUser);
            ActiveMenu = "Product";
        }

        private void ExecuteNavigateStockHistory()
        {
            CurrentView = new StockHistory(_currentUser);
            ActiveMenu = "StockHistory";
        }

        private void ExecuteLogout()
        {
            // Close the admin dashboard first
            Window? adminWindow = null;
            foreach (Window window in Application.Current.Windows)
            {
                if (window is Views.Admin.AdminDashboard)
                {
                    adminWindow = window;
                    break;
                }
            }

            // Create and show the login window
            var loginWindow = new MainWindow();
            loginWindow.Show();

            // Now close the admin dashboard
            adminWindow?.Close();
        }
    }
}
