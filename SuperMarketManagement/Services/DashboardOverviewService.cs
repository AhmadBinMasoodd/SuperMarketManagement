using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SuperMarketManagement.Models;

namespace SuperMarketManagement.Services
{
    public sealed class DashboardOverviewService
    {
        public DashboardOverviewSnapshot GetSnapshot()
        {
            using var context = new MarketDbContext();

            var totalSales = context.Sales.AsNoTracking().Count();
            var totalRevenue = context.Sales.AsNoTracking().Sum(s => (decimal?)s.TotalAmount) ?? 0m;
            var totalProducts = context.Products.AsNoTracking().Count();
            var totalUsers = context.Users.AsNoTracking().Count();
            var totalStock = context.Products.AsNoTracking().Sum(p => (decimal?)p.Quantity) ?? 0m;
            var lowStockThreshold = 10m;
            var lowStockProducts = context.Products.AsNoTracking().Count(p => p.Quantity <= lowStockThreshold);
            var recentSales = (from sale in context.Sales.AsNoTracking()
                               join user in context.Users.AsNoTracking() on sale.UserId equals user.Id into userJoin
                               from user in userJoin.DefaultIfEmpty()
                               orderby sale.SaleDateTime descending
                               select new DashboardRecentSale
                               {
                                   SaleId = sale.Id,
                                   SaleDateTime = sale.SaleDateTime,
                                   CashierName = user != null ? user.Name : "Unknown",
                                   TotalAmount = sale.TotalAmount
                               }).Take(6).ToList();

            var recentStock = (from stock in context.StockTransactions.AsNoTracking()
                               join product in context.Products.AsNoTracking() on stock.ProductId equals product.Id into productJoin
                               from product in productJoin.DefaultIfEmpty()
                               join user in context.Users.AsNoTracking() on stock.UserId equals user.Id into userJoin
                               from user in userJoin.DefaultIfEmpty()
                               orderby stock.TransactionDateTime descending
                               select new DashboardStockMovement
                               {
                                   TransactionDateTime = stock.TransactionDateTime,
                                   ProductName = product != null ? product.Name : "Unknown",
                                   UserName = user != null ? user.Name : "Unknown",
                                   TransactionType = stock.TransactionType,
                                   QuantityChanged = stock.QuantityChanged,
                                   Remarks = stock.Remarks
                               }).Take(6).ToList();

            var lowStockItems = context.Products.AsNoTracking()
                .Where(p => p.Quantity <= lowStockThreshold)
                .OrderBy(p => p.Quantity)
                .ThenBy(p => p.Name)
                .Select(p => new DashboardLowStockItem
                {
                    ProductName = p.Name,
                    Quantity = p.Quantity,
                    SalePrice = p.SalePrice
                })
                .Take(6)
                .ToList();

            return new DashboardOverviewSnapshot
            {
                TotalSales = totalSales,
                TotalRevenue = totalRevenue,
                TotalProducts = totalProducts,
                TotalUsers = totalUsers,
                TotalStock = totalStock,
                LowStockProducts = lowStockProducts,
                LowStockThreshold = lowStockThreshold,
                RecentSales = recentSales,
                RecentStockMovements = recentStock,
                LowStockItems = lowStockItems
            };
        }
    }

    public sealed class DashboardOverviewSnapshot
    {
        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalProducts { get; set; }
        public int TotalUsers { get; set; }
        public decimal TotalStock { get; set; }
        public int LowStockProducts { get; set; }
        public decimal LowStockThreshold { get; set; }
        public IReadOnlyList<DashboardRecentSale> RecentSales { get; set; } = Array.Empty<DashboardRecentSale>();
        public IReadOnlyList<DashboardStockMovement> RecentStockMovements { get; set; } = Array.Empty<DashboardStockMovement>();
        public IReadOnlyList<DashboardLowStockItem> LowStockItems { get; set; } = Array.Empty<DashboardLowStockItem>();
    }

    public sealed class DashboardRecentSale
    {
        public int SaleId { get; set; }
        public DateTime SaleDateTime { get; set; }
        public string CashierName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }

    public sealed class DashboardStockMovement
    {
        public DateTime TransactionDateTime { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal QuantityChanged { get; set; }
        public string? Remarks { get; set; }
    }

    public sealed class DashboardLowStockItem
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal SalePrice { get; set; }
    }
}