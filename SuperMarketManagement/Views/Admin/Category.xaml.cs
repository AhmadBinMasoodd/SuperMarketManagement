using System.Windows.Controls;
using SuperMarketManagement.ViewModels;

namespace SuperMarketManagement.Views.Admin
{
    /// <summary>
    /// Interaction logic for Category.xaml
    /// </summary>
    public partial class Category : UserControl
    {
        private CategoryViewModel? _viewModel;

        public Category()
        {
            InitializeComponent();
            _viewModel = new CategoryViewModel();
            DataContext = _viewModel;
        }

        ~Category()
        {
            _viewModel?.Dispose();
        }
    }
}
