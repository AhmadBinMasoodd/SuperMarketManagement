using System.Windows;
using System.Windows.Controls;
using SuperMarketManagement.Services;

namespace SuperMarketManagement.Views.Cashier
{
    public partial class ReturnRefundView : UserControl
    {
        public ReturnRefundView()
        {
            InitializeComponent();
        }


        public void ClearSearch()
        {
            SearchTextBox.Text = string.Empty;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(SearchTextBox.Text, out var saleId))
            {
                MessageBox.Show("Please enter a valid receipt ID.", "Return & Refund", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var refundService = new RefundService();
            var receipt = refundService.GetReceiptDetails(saleId);

            if (receipt is null)
            {
                MessageBox.Show($"Receipt {saleId} was not found.", "Return & Refund", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBox.Show(
                $"Receipt {receipt.SaleId} loaded. Items: {receipt.Items.Count}. Total: {receipt.TotalAmount:N2}",
                "Return & Refund",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

    }
}
