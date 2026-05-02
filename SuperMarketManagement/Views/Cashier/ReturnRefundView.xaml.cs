using System.Windows.Controls;
using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels;

namespace SuperMarketManagement.Views.Cashier
{
    public partial class ReturnRefundView : UserControl
    {
        public ReturnRefundView(User user)
        {
            InitializeComponent();
            DataContext = new ReturnRefundViewModel(user);
        }
    }
}
