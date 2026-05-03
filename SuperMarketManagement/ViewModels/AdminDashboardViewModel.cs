using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels.Base;
using SuperMarketManagement.Views.Admin;
using SuperMarketManagement.Views.Cashier;
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
        public ICommand NavigatePosCommand { get; }
        public ICommand NavigateReturnRefundCommand { get; }
        public ICommand NavigateDailySummaryCommand { get; }
        public ICommand NavigateProfileCommand { get; }
        public ICommand LogoutCommand { get; }

        public AdminDashboardViewModel(User user)
        {
            _currentUser = user;

            NavigateDashboardCommand = new RelayCommand(_ => ExecuteNavigateDashboard());
            NavigateEmployeesCommand = new RelayCommand(_ => ExecuteNavigateEmployees());
            NavigateCategoryCommand = new RelayCommand(_ => ExecuteNavigateCategory());
            NavigateProductCommand = new RelayCommand(_ => ExecuteNavigateProduct());
            NavigateStockHistoryCommand = new RelayCommand(_ => ExecuteNavigateStockHistory());
            NavigatePosCommand = new RelayCommand(_ => ExecuteNavigatePos());
            NavigateReturnRefundCommand = new RelayCommand(_ => ExecuteNavigateReturnRefund());
            NavigateDailySummaryCommand = new RelayCommand(_ => ExecuteNavigateDailySummary());
            NavigateProfileCommand = new RelayCommand(_ => ExecuteNavigateProfile());
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

        private void ExecuteNavigatePos()
        {
            CurrentView = new SalesView(_currentUser);
            ActiveMenu = "POS";
        }

        private void ExecuteNavigateReturnRefund()
        {
            CurrentView = new ReturnRefundView(_currentUser);
            ActiveMenu = "ReturnRefund";
        }

        private void ExecuteNavigateDailySummary()
        {
            CurrentView = new DailySummaryView(_currentUser);
            ActiveMenu = "DailySummary";
        }

        private void ExecuteNavigateProfile()
        {
            CurrentView = new Profile(_currentUser);
            ActiveMenu = "Profile";
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
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();

            // Now close the admin dashboard
            adminWindow?.Close();
        }
    }
}
