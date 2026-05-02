using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SuperMarketManagement.Models;

namespace SuperMarketManagement.Services
{
    public sealed class ManagerDashboardOverviewService
    {
        public ManagerDashboardOverviewSnapshot GetSnapshot()
        {
            using var context = new MarketDbContext();

            var lowStockThreshold = 10m;
            var totalProducts = context.Products.AsNoTracking().Count();
            var totalCategories = context.Categories.AsNoTracking().Count();
            var totalStock = context.Products.AsNoTracking().Sum(p => (decimal?)p.Quantity) ?? 0m;
            var lowStockProducts = context.Products.AsNoTracking().Count(p => p.Quantity > 0 && p.Quantity <= lowStockThreshold);
            var outOfStockProducts = context.Products.AsNoTracking().Count(p => p.Quantity <= 0);

            var recentStockMovements = (from stock in context.StockTransactions.AsNoTracking()
                                        join product in context.Products.AsNoTracking() on stock.ProductId equals product.Id into productJoin
                                        from product in productJoin.DefaultIfEmpty()
                                        join user in context.Users.AsNoTracking() on stock.UserId equals user.Id into userJoin
                                        from user in userJoin.DefaultIfEmpty()
                                        orderby stock.TransactionDateTime descending
                                        select new ManagerDashboardStockMovement
                                        {
                                            TransactionDateTime = stock.TransactionDateTime,
                                            ProductName = product != null ? product.Name : "Unknown",
                                            UserName = user != null ? user.Name : "Unknown",
                                            TransactionType = stock.TransactionType,
                                            QuantityChanged = stock.QuantityChanged,
                                            Remarks = stock.Remarks
                                        }).Take(8).ToList();

            var lowStockItems = context.Products.AsNoTracking()
                .Where(p => p.Quantity <= lowStockThreshold)
                .OrderBy(p => p.Quantity)
                .ThenBy(p => p.Name)
                .Select(p => new ManagerDashboardLowStockItem
                {
                    ProductName = p.Name,
                    Quantity = p.Quantity,
                    SalePrice = p.SalePrice,
                    PurchasePrice = p.PurchasePrice
                })
                .Take(8)
                .ToList();

            return new ManagerDashboardOverviewSnapshot
            {
                TotalProducts = totalProducts,
                TotalCategories = totalCategories,
                TotalStock = totalStock,
                LowStockProducts = lowStockProducts,
                OutOfStockProducts = outOfStockProducts,
                LowStockThreshold = lowStockThreshold,
                RecentStockMovements = recentStockMovements,
                LowStockItems = lowStockItems
            };
        }
    }

    public sealed class ManagerDashboardOverviewSnapshot
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public decimal TotalStock { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public decimal LowStockThreshold { get; set; }
        public IReadOnlyList<ManagerDashboardStockMovement> RecentStockMovements { get; set; } = Array.Empty<ManagerDashboardStockMovement>();
        public IReadOnlyList<ManagerDashboardLowStockItem> LowStockItems { get; set; } = Array.Empty<ManagerDashboardLowStockItem>();
    }

    public sealed class ManagerDashboardStockMovement
    {
        public DateTime TransactionDateTime { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal QuantityChanged { get; set; }
        public string? Remarks { get; set; }
    }

    public sealed class ManagerDashboardLowStockItem
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal SalePrice { get; set; }
        public decimal PurchasePrice { get; set; }
    }
}