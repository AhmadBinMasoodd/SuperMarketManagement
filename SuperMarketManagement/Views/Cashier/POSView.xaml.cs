using System.Windows.Controls;
using SuperMarketManagement.Models;

namespace SuperMarketManagement.Views.Cashier
{
    public partial class POSView : UserControl
    {
        private readonly User _user;

        public POSView(User user)
        {
            InitializeComponent();
            _user = user;
        }
    }
}
