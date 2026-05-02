using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using SuperMarketManagement.Services;
using SuperMarketManagement.ViewModels.Base;

namespace SuperMarketManagement.ViewModels
{
    public sealed class ManagerChartOverviewViewModel : ViewModelBase
    {
        private readonly ManagerDashboardOverviewService _dashboardService = new();

        private string _welcomeMessage = "Welcome back";
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        private string _subtitle = "Track stock health, product coverage, and recent inventory movement.";
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

        private ObservableCollection<ManagerDashboardStockMovement> _recentStockMovements = new();
        public ObservableCollection<ManagerDashboardStockMovement> RecentStockMovements
        {
            get => _recentStockMovements;
            set => SetProperty(ref _recentStockMovements, value);
        }

        private ObservableCollection<ManagerDashboardLowStockItem> _lowStockItems = new();
        public ObservableCollection<ManagerDashboardLowStockItem> LowStockItems
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

        public ManagerChartOverviewViewModel()
        {
            LoadSnapshot();
        }

        private void LoadSnapshot()
        {
            var snapshot = _dashboardService.GetSnapshot();

            HeroStat = $"{snapshot.LowStockProducts} low stock items";

            Metrics = new ObservableCollection<DashboardMetricCardViewModel>
            {
                new()
                {
                    Title = "Categories",
                    Value = snapshot.TotalCategories.ToString(),
                    Description = "Product group coverage",
                    Accent = new SolidColorBrush(Color.FromRgb(20, 184, 166))
                },
                new()
                {
                    Title = "Products",
                    Value = snapshot.TotalProducts.ToString(),
                    Description = "Catalog items managed",
                    Accent = new SolidColorBrush(Color.FromRgb(99, 62, 208))
                },
                new()
                {
                    Title = "Stock On Hand",
                    Value = snapshot.TotalStock.ToString("N0"),
                    Description = "Units currently available",
                    Accent = new SolidColorBrush(Color.FromRgb(245, 158, 11))
                },
                new()
                {
                    Title = "Low Stock",
                    Value = snapshot.LowStockProducts.ToString(),
                    Description = $"At or below {snapshot.LowStockThreshold:N0} units",
                    Accent = new SolidColorBrush(Color.FromRgb(239, 68, 68))
                },
                new()
                {
                    Title = "Out of Stock",
                    Value = snapshot.OutOfStockProducts.ToString(),
                    Description = "Items that need replenishment",
                    Accent = new SolidColorBrush(Color.FromRgb(37, 99, 235))
                }
            };

            RecentStockMovements = new ObservableCollection<ManagerDashboardStockMovement>(snapshot.RecentStockMovements);
            LowStockItems = new ObservableCollection<ManagerDashboardLowStockItem>(snapshot.LowStockItems);
        }
    }
}