using System.Windows.Controls;
using SuperMarketManagement.ViewModels;

namespace SuperMarketManagement.Views.Manager
{
    public partial class ManagerChartOverview : UserControl
    {
        public ManagerChartOverview()
        {
            InitializeComponent();
            DataContext = new ManagerChartOverviewViewModel();
        }
    }
}