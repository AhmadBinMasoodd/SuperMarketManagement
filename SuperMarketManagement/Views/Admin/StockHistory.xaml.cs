using System.Windows.Controls;
using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels;

namespace SuperMarketManagement.Views.Admin
{
    public partial class StockHistory : UserControl
    {
        public StockHistory(User user)
        {
            InitializeComponent();
            DataContext = new StockHistoryViewModel();
        }
    }
}

