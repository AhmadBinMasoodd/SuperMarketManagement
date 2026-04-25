using SuperMarketManagement.Controller;
using SuperMarketManagement.Views.Admin;
using SuperMarketManagement.Views.Manager;
using System.Windows;

namespace SuperMarketManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly LoginController _loginController = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = EmailBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorText.Text = "Please enter username and password.";
                return;
            }

            var user = _loginController.Authenticate(username, password);
            if (user is null)
            {
                ErrorText.Text = "Invalid credentials.";
                return;
            }

            ErrorText.Text = string.Empty;

            if (user.Role == "Admin")
            {
                var dashboard = new AdminDashboard(user);
                dashboard.Show();
                Close();
                return;
            }

            if (user.Role == "Manager")
            {
                var dashboard = new ManagerDashboard(user);
                dashboard.Show();
                Close();
                return;
            }
            ErrorText.Text = "Role not supported yet.";
        }
    }
}