using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels.Base;
using SuperMarketManagement.Views.Admin;
using SuperMarketManagement.Views.Cashier;

namespace SuperMarketManagement.ViewModels
{
    public sealed class CashierDashboardViewModel : ViewModelBase
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

        public string CashierName => _currentUser.Name;
        public string CashierRole => _currentUser.Role;

        public ICommand NavigatePosCommand { get; }
        public ICommand NavigateReturnRefundCommand { get; }
        public ICommand NavigateDailySummaryCommand { get; }
        public ICommand NavigateProfileCommand { get; }
        public ICommand LogoutCommand { get; }

        public CashierDashboardViewModel(User user)
        {
            _currentUser = user;

            NavigatePosCommand = new RelayCommand(_ => ExecuteNavigatePos());
            NavigateReturnRefundCommand = new RelayCommand(_ => ExecuteNavigateReturnRefund());
            NavigateDailySummaryCommand = new RelayCommand(_ => ExecuteNavigateDailySummary());
            NavigateProfileCommand = new RelayCommand(_ => ExecuteNavigateProfile());
            LogoutCommand = new RelayCommand(_ => ExecuteLogout());

            ExecuteNavigatePos();
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