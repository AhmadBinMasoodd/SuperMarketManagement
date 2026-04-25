using System.Windows.Controls;
using SuperMarketManagement.Controller;
using SuperMarketManagement.Models;

namespace SuperMarketManagement.Views.Admin
{
    public partial class StockHistory : UserControl
    {
        private readonly ProductController _controller = new();

        public StockHistory(User user)
        {
            InitializeComponent();
            LoadHistory();
        }

        ~StockHistory()
        {
            _controller.Dispose();
        }

        private void LoadHistory(string? search = null)
        {
            var grid = FindName("HistoryDataGrid") as DataGrid;
            if (grid != null)
            {
                grid.ItemsSource = _controller.GetStockHistory(search);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchBox = sender as TextBox;
            LoadHistory(searchBox?.Text);
        }
    }
}
