using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SuperMarketManagement.Models;
using SuperMarketManagement.Services;
using SuperMarketManagement.ViewModels.Base;

namespace SuperMarketManagement.ViewModels
{
    public sealed class CashierDailySummaryViewModel : ViewModelBase, IDisposable
    {
        private readonly CashierDailySummaryService _summaryService = new();
        private readonly User _currentUser;

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value.Date))
                {
                    LoadSummary();
                }
            }
        }

        private decimal _totalSalesAmount;
        public decimal TotalSalesAmount
        {
            get => _totalSalesAmount;
            set => SetProperty(ref _totalSalesAmount, value);
        }

        private int _receiptCount;
        public int ReceiptCount
        {
            get => _receiptCount;
            set => SetProperty(ref _receiptCount, value);
        }

        private decimal _totalItemsSold;
        public decimal TotalItemsSold
        {
            get => _totalItemsSold;
            set => SetProperty(ref _totalItemsSold, value);
        }

        private decimal _averageReceiptAmount;
        public decimal AverageReceiptAmount
        {
            get => _averageReceiptAmount;
            set => SetProperty(ref _averageReceiptAmount, value);
        }

        private decimal _highestReceiptAmount;
        public decimal HighestReceiptAmount
        {
            get => _highestReceiptAmount;
            set => SetProperty(ref _highestReceiptAmount, value);
        }

        private string _summaryTitle = string.Empty;
        public string SummaryTitle
        {
            get => _summaryTitle;
            set => SetProperty(ref _summaryTitle, value);
        }

        private bool _hasSales;
        public bool HasSales
        {
            get => _hasSales;
            set
            {
                if (SetProperty(ref _hasSales, value))
                {
                    OnPropertyChanged(nameof(NoSalesMessageVisibility));
                    OnPropertyChanged(nameof(ReceiptsVisibility));
                }
            }
        }

        public System.Windows.Visibility NoSalesMessageVisibility => HasSales ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        public System.Windows.Visibility ReceiptsVisibility => HasSales ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        private ObservableCollection<CashierReceiptRowViewModel> _receipts = new();
        public ObservableCollection<CashierReceiptRowViewModel> Receipts
        {
            get => _receipts;
            set => SetProperty(ref _receipts, value);
        }

        public ICommand TodayCommand { get; }

        public CashierDailySummaryViewModel(User user)
        {
            _currentUser = user;
            TodayCommand = new RelayCommand(_ => SelectedDate = DateTime.Today);
            LoadSummary();
        }

        private void LoadSummary()
        {
            var snapshot = _summaryService.GetSummary(_currentUser.Id, SelectedDate);

            TotalSalesAmount = snapshot.TotalSalesAmount;
            ReceiptCount = snapshot.ReceiptCount;
            TotalItemsSold = snapshot.TotalItemsSold;
            AverageReceiptAmount = snapshot.AverageReceiptAmount;
            HighestReceiptAmount = snapshot.HighestReceiptAmount;

            SummaryTitle = SelectedDate.Date == DateTime.Today
                ? "Today Summary"
                : $"Summary for {SelectedDate:dd MMM yyyy}";

            var rows = new ObservableCollection<CashierReceiptRowViewModel>();
            for (var i = 0; i < snapshot.Sales.Count; i++)
            {
                var sale = snapshot.Sales[i];
                rows.Add(new CashierReceiptRowViewModel
                {
                    SrNo = i + 1,
                    ReceiptNo = sale.SaleId,
                    Time = sale.SaleDateTime.ToString("hh:mm tt"),
                    Items = sale.ItemsCount,
                    TotalAmount = sale.TotalAmount
                });
            }

            Receipts = rows;
            HasSales = rows.Count > 0;
        }

        public void Dispose() => _summaryService.Dispose();
    }

    public sealed class CashierReceiptRowViewModel
    {
        public int SrNo { get; set; }
        public int ReceiptNo { get; set; }
        public string Time { get; set; } = string.Empty;
        public decimal Items { get; set; }
        public decimal TotalAmount { get; set; }
    }
}