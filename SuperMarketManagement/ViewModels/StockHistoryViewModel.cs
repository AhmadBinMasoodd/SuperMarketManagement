using System;
using System.Collections.ObjectModel;
using System.Linq;
using SuperMarketManagement.Models;
using SuperMarketManagement.Services;
using SuperMarketManagement.ViewModels.Base;

namespace SuperMarketManagement.ViewModels
{
    public class StockHistoryViewModel : ViewModelBase, IDisposable
    {
        private readonly StockHistoryService _stockHistoryService = new();

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

        private void LoadHistory()
        {
            History = new ObservableCollection<StockHistoryGridItemViewModel>(_stockHistoryService.GetHistory(SearchText));
        }

        public void Dispose() => _stockHistoryService.Dispose();
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
