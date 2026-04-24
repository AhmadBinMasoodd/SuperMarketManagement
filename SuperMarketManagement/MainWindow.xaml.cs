using SuperMarketManagement.Controller;
using SuperMarketManagement.Views.Admin;
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

            var role = _loginController.Authenticate(username, password);
            if (string.IsNullOrWhiteSpace(role))
            {
                ErrorText.Text = "Invalid credentials.";
                return;
            }

            ErrorText.Text = string.Empty;

            if (role == "Admin")
            {
                var dashboard = new AdminDashboard();
                dashboard.Show();
                Close();
                return;
            }

            if (role == "Manager")
            {

            }
            ErrorText.Text = "Role not supported yet.";
        }
    }
}