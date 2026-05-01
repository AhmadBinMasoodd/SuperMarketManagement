using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using SuperMarketManagement.Services;
using SuperMarketManagement.ViewModels.Base;

namespace SuperMarketManagement.ViewModels
{
    public sealed class ChartOverviewViewModel : ViewModelBase
    {
        private readonly DashboardOverviewService _dashboardService = new();

        private string _welcomeMessage = "Welcome back";
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        private string _subtitle = "Live sales, stock health, and recent activity at a glance.";
        public string Subtitle
        {
            get => _subtitle;
            set => SetProperty(ref _subtitle, value);
        }

        private ObservableCollection<DashboardMetricCardViewModel> _metrics = new();
        public ObservableCollection<DashboardMetricCardViewModel> Metrics
        {
            get => _metrics;
            set => SetProperty(ref _metrics, value);
        }

        private ObservableCollection<DashboardRecentSaleViewModel> _recentSales = new();
        public ObservableCollection<DashboardRecentSaleViewModel> RecentSales
        {
            get => _recentSales;
            set => SetProperty(ref _recentSales, value);
        }

        private ObservableCollection<DashboardStockMovementViewModel> _recentStockMovements = new();
        public ObservableCollection<DashboardStockMovementViewModel> RecentStockMovements
        {
            get => _recentStockMovements;
            set => SetProperty(ref _recentStockMovements, value);
        }

        private ObservableCollection<DashboardLowStockViewModel> _lowStockItems = new();
        public ObservableCollection<DashboardLowStockViewModel> LowStockItems
        {
            get => _lowStockItems;
            set => SetProperty(ref _lowStockItems, value);
        }

        private string _heroStat = string.Empty;
        public string HeroStat
        {
            get => _heroStat;
            set => SetProperty(ref _heroStat, value);
        }

        public ChartOverviewViewModel()
        {
            LoadSnapshot();
        }

        private void LoadSnapshot()
        {
            var snapshot = _dashboardService.GetSnapshot();

            HeroStat = $"{snapshot.TotalRevenue:N2}";

            Metrics = new ObservableCollection<DashboardMetricCardViewModel>
            {
                new()
                {
                    Title = "Total Sales",
                    Value = snapshot.TotalSales.ToString(),
                    Description = "Completed transactions",
                    Accent = new SolidColorBrush(Color.FromRgb(99, 62, 208))
                },
                new()
                {
                    Title = "Revenue",
                    Value = snapshot.TotalRevenue.ToString("N2"),
                    Description = "Gross sales value",
                    Accent = new SolidColorBrush(Color.FromRgb(14, 165, 233))
                },
                new()
                {
                    Title = "Products",
                    Value = snapshot.TotalProducts.ToString(),
                    Description = "Catalog items live",
                    Accent = new SolidColorBrush(Color.FromRgb(34, 197, 94))
                },
                new()
                {
                    Title = "Stock On Hand",
                    Value = snapshot.TotalStock.ToString("N0"),
                    Description = "Units currently in store",
                    Accent = new SolidColorBrush(Color.FromRgb(245, 158, 11))
                },
                new()
                {
                    Title = "Low Stock",
                    Value = snapshot.LowStockProducts.ToString(),
                    Description = $"Items at or below {snapshot.LowStockThreshold:N0}",
                    Accent = new SolidColorBrush(Color.FromRgb(239, 68, 68))
                },
                new()
                {
                    Title = "Team Members",
                    Value = snapshot.TotalUsers.ToString(),
                    Description = "Active system users",
                    Accent = new SolidColorBrush(Color.FromRgb(20, 184, 166))
                }
            };

            RecentSales = new ObservableCollection<DashboardRecentSaleViewModel>(snapshot.RecentSales.Select(item => new DashboardRecentSaleViewModel
            {
                SaleId = item.SaleId,
                SaleDateTime = item.SaleDateTime,
                CashierName = item.CashierName,
                TotalAmount = item.TotalAmount
            }));

            RecentStockMovements = new ObservableCollection<DashboardStockMovementViewModel>(snapshot.RecentStockMovements.Select(item => new DashboardStockMovementViewModel
            {
                TransactionDateTime = item.TransactionDateTime,
                ProductName = item.ProductName,
                UserName = item.UserName,
                TransactionType = item.TransactionType,
                QuantityChanged = item.QuantityChanged,
                Remarks = item.Remarks
            }));

            LowStockItems = new ObservableCollection<DashboardLowStockViewModel>(snapshot.LowStockItems.Select(item => new DashboardLowStockViewModel
            {
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                SalePrice = item.SalePrice
            }));
        }
    }

    public sealed class DashboardMetricCardViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Brush Accent { get; set; } = Brushes.SlateBlue;
    }

    public sealed class DashboardRecentSaleViewModel
    {
        public int SaleId { get; set; }
        public DateTime SaleDateTime { get; set; }
        public string CashierName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }

    public sealed class DashboardStockMovementViewModel
    {
        public DateTime TransactionDateTime { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal QuantityChanged { get; set; }
        public string? Remarks { get; set; }
    }

    public sealed class DashboardLowStockViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal SalePrice { get; set; }
    }
}