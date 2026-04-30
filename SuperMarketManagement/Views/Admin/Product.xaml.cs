using System.Windows.Controls;
using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels;

namespace SuperMarketManagement.Views.Admin
{
    public partial class Product : UserControl
    {
        public Product(User user)
        {
            InitializeComponent();
            DataContext = new ProductViewModel(user);
        }
    }
}
