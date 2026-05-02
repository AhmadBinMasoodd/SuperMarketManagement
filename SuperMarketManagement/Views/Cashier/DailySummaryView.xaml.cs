using System.Windows.Controls;
using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels;

namespace SuperMarketManagement.Views.Cashier
{
    public partial class DailySummaryView : UserControl
    {
        public DailySummaryView(User user)
        {
            InitializeComponent();
            DataContext = new CashierDailySummaryViewModel(user);
        }
    }
}
