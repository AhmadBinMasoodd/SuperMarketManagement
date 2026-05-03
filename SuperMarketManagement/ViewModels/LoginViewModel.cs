using SuperMarketManagement.ViewModels.Base;
using SuperMarketManagement.Views.Admin;
using SuperMarketManagement.Views.Manager;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using SuperMarketManagement.Models;
using SuperMarketManagement.Services;

namespace SuperMarketManagement.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly AuthService _authService = new();

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        // Password is not bound directly via standard TwoWay binding for security reasons,
        // but for a simple migration, we can pass it via CommandParameter from the view,
        // or bind it using an attached property/helper.
        // The easiest MVVM approach for PasswordBox is to pass the PasswordBox to the command.

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        private void ExecuteLogin(object? parameter)
        {
            // parameter is the PasswordBox from the View
            var passwordBox = parameter as System.Windows.Controls.PasswordBox;
            var password = passwordBox?.Password;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Please enter username and password.";
                return;
            }

            var user = _authService.Authenticate(Email, password);
            if (user is null)
            {
                ErrorMessage = "Invalid credentials.";
                return;
            }

            ErrorMessage = string.Empty;

                // Find the currently displayed login window (if any) so we can close it
                var loginWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

                if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    var dashboard = new AdminDashboard(user);
                    Application.Current.MainWindow = dashboard;
                    dashboard.Show();
                    loginWindow?.Close();
                    return;
                }

                if (string.Equals(user.Role, "Manager", StringComparison.OrdinalIgnoreCase))
                {
                    var dashboard = new ManagerDashboard(user);
                    Application.Current.MainWindow = dashboard;
                    dashboard.Show();
                    loginWindow?.Close();
                    return;
                }

                if (string.Equals(user.Role, "Cashier", StringComparison.OrdinalIgnoreCase))
                {
                    var dashboard = new SuperMarketManagement.Views.Cashier.CashierDashboard(user);
                    Application.Current.MainWindow = dashboard;
                    dashboard.Show();
                    loginWindow?.Close();
                    return;
                }

            ErrorMessage = $"Role '{user.Role}' not supported yet.";
        }
    }
}
