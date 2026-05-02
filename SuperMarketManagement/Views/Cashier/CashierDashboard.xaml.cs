using System.Windows;
using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels;

namespace SuperMarketManagement.Views.Cashier
{
    public partial class CashierDashboard : Window
    {
        public CashierDashboard(User user)
        {
            InitializeComponent();
            DataContext = new CashierDashboardViewModel(user);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
