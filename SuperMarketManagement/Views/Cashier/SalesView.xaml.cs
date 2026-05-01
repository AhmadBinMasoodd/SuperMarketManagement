using System.Windows.Controls;
using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels;

namespace SuperMarketManagement.Views.Cashier
{
    public partial class SalesView : UserControl
    {
        private readonly User _user;

        public SalesView(User user)
        {
            InitializeComponent();
            _user = user;
            DataContext = new SalesViewModel(_user);
        }
    }
}
