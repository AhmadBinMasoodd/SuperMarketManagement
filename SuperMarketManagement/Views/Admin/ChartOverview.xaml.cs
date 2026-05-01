using System.Windows.Controls;
using SuperMarketManagement.ViewModels;

namespace SuperMarketManagement.Views.Admin
{
    public partial class ChartOverview : UserControl
    {
        public ChartOverview()
        {
            InitializeComponent();
            DataContext = new ChartOverviewViewModel();
        }
    }
}
