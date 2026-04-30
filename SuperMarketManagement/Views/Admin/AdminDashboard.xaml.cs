using System.Windows;
using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels;

namespace SuperMarketManagement.Views.Admin
{
    /// <summary>
    /// Interaction logic for AdminDashboard.xaml
    /// </summary>
    public partial class AdminDashboard : Window
    {
        public AdminDashboard(User user)
        {
            InitializeComponent();
            DataContext = new AdminDashboardViewModel(user);
        }
    }
}
