using System;
using System.Collections.ObjectModel;
using System.Linq;
using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels.Base;

namespace SuperMarketManagement.ViewModels
{
    public class StockHistoryViewModel : ViewModelBase, IDisposable
    {
        private readonly MarketDbContext _context = new();

        private ObservableCollection<StockHistoryGridItemViewModel> _history = new();
        public ObservableCollection<StockHistoryGridItemViewModel> History
        {
            get => _history;
            set => SetProperty(ref _history, value);
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    LoadHistory();
                }
            }
        }

        public StockHistoryViewModel()
        {
            LoadHistory();
        }

        private static string NormalizeTransactionType(string? transactionType)
        {
            if (string.IsNullOrWhiteSpace(transactionType))
            {
                return "Unknown";
            }

            if (transactionType.Contains("damag", StringComparison.OrdinalIgnoreCase))
            {
                return "Stock Damaged";
            }

            if (transactionType.Contains("out", StringComparison.OrdinalIgnoreCase))
            {
                return "Stock Out";
            }

            if (transactionType.Contains("in", StringComparison.OrdinalIgnoreCase))
            {
                return "Stock In";
            }

            return transactionType;
        }

        private void LoadHistory()
        {
            var query = _context.StockTransactions
                .OrderByDescending(st => st.TransactionDateTime)
                .Select(st => new StockHistoryGridItemViewModel
                {
                    TransactionDateTime = st.TransactionDateTime,
                    ProductName = _context.Products.Where(p => p.Id == st.ProductId).Select(p => p.Name).FirstOrDefault() ?? "Unknown",
                    UserName = _context.Users.Where(u => u.Id == st.UserId).Select(u => u.Name).FirstOrDefault() ?? "Unknown",
                    TransactionType = NormalizeTransactionType(st.TransactionType),
                    QuantityChanged = st.QuantityChanged,
                    Remarks = st.Remarks
                });

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var text = SearchText.Trim().ToLower();
                query = query.Where(x => x.ProductName.ToLower().Contains(text) || x.UserName.ToLower().Contains(text));
            }

            History = new ObservableCollection<StockHistoryGridItemViewModel>(query.ToList());
        }

        public void Dispose() => _context.Dispose();
    }

    public class StockHistoryGridItemViewModel
    {
        public DateTime TransactionDateTime { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal QuantityChanged { get; set; }
        public string? Remarks { get; set; }
    }
}
