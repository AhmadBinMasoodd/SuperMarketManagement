using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels.Base;
using SuperMarketManagement.Views.Admin;

namespace SuperMarketManagement.ViewModels
{
    public sealed class ManagerDashboardViewModel : ViewModelBase
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

        public string ManagerName => _currentUser.Name;
        public string ManagerRole => _currentUser.Role;

        public ICommand NavigateDashboardCommand { get; }
        public ICommand NavigateCategoryCommand { get; }
        public ICommand NavigateProductCommand { get; }
        public ICommand NavigateStockHistoryCommand { get; }
        public ICommand LogoutCommand { get; }

        public ManagerDashboardViewModel(User user)
        {
            _currentUser = user;

            NavigateDashboardCommand = new RelayCommand(_ => ExecuteNavigateDashboard());
            NavigateCategoryCommand = new RelayCommand(_ => ExecuteNavigateCategory());
            NavigateProductCommand = new RelayCommand(_ => ExecuteNavigateProduct());
            NavigateStockHistoryCommand = new RelayCommand(_ => ExecuteNavigateStockHistory());
            LogoutCommand = new RelayCommand(_ => ExecuteLogout());

            ExecuteNavigateDashboard();
        }

        private void ExecuteNavigateDashboard()
        {
            CurrentView = new ChartOverview();
            ActiveMenu = "Dashboard";
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
            var loginWindow = new MainWindow();
            loginWindow.Show();

            foreach (Window window in Application.Current.Windows)
            {
                if (ReferenceEquals(window.DataContext, this))
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}