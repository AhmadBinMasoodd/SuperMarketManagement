using System.Windows;
using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels;

namespace SuperMarketManagement.Views.Manager
{
    public partial class ManagerDashboard : Window
    {
        public ManagerDashboard(User user)
        {
            InitializeComponent();
            DataContext = new AdminDashboardViewModel(user);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}

