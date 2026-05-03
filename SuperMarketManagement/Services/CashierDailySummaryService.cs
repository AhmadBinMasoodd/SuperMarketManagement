using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SuperMarketManagement.Models;

namespace SuperMarketManagement.Services
{
    public sealed class CashierDailySummaryService : IDisposable
    {
        private readonly MarketDbContext _context = new();

        public CashierDailySummarySnapshot GetSummary(int viewerUserId, DateTime date)
        {
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);

            // Determine which users' sales the viewer is allowed to see
            var viewer = _context.Users.AsNoTracking().FirstOrDefault(u => u.Id == viewerUserId);
            if (viewer is null)
            {
                return new CashierDailySummarySnapshot
                {
                    Date = dayStart,
                    TotalSalesAmount = 0m,
                    ReceiptCount = 0,
                    TotalItemsSold = 0m,
                    AverageReceiptAmount = 0m,
                    HighestReceiptAmount = 0m,
                    Sales = Array.Empty<CashierReceiptSummary>()
                };
            }

            List<int> allowedUserIds;
            var role = (viewer.Role ?? string.Empty).ToLowerInvariant();
            if (role == "admin")
            {
                allowedUserIds = _context.Users.AsNoTracking().Select(u => u.Id).ToList();
            }
            else if (role == "manager")
            {
                allowedUserIds = _context.Users.AsNoTracking()
                    .Where(u => u.Role.ToLower() == "manager" || u.Role.ToLower() == "cashier")
                    .Select(u => u.Id)
                    .ToList();
            }
            else
            {
                // default: cashier or other roles can only see their own sales
                allowedUserIds = new List<int> { viewerUserId };
            }

            var sales = _context.Sales
                .AsNoTracking()
                .Where(s => allowedUserIds.Contains(s.UserId) && s.SaleDateTime >= dayStart && s.SaleDateTime < dayEnd)
                .OrderByDescending(s => s.SaleDateTime)
                .ToList();

            if (sales.Count == 0)
            {
                return new CashierDailySummarySnapshot
                {
                    Date = dayStart,
                    TotalSalesAmount = 0m,
                    ReceiptCount = 0,
                    TotalItemsSold = 0m,
                    AverageReceiptAmount = 0m,
                    HighestReceiptAmount = 0m,
                    Sales = Array.Empty<CashierReceiptSummary>()
                };
            }

            var saleIds = sales.Select(s => s.Id).ToList();
            var itemTotalsBySale = _context.SaleItems
                .AsNoTracking()
                .Where(i => saleIds.Contains(i.SaleId))
                .GroupBy(i => i.SaleId)
                .Select(g => new
                {
                    SaleId = g.Key,
                    Items = g.Sum(x => x.Quantity)
                })
                .ToDictionary(x => x.SaleId, x => x.Items);

            var receiptRows = sales.Select(s => new CashierReceiptSummary
            {
                SaleId = s.Id,
                SaleDateTime = s.SaleDateTime,
                TotalAmount = s.TotalAmount,
                ItemsCount = itemTotalsBySale.TryGetValue(s.Id, out var qty) ? qty : 0m
            }).ToList();

            var totalSalesAmount = sales.Sum(s => s.TotalAmount);
            var receiptCount = sales.Count;
            var totalItems = receiptRows.Sum(r => r.ItemsCount);
            var highestReceipt = sales.Max(s => s.TotalAmount);

            return new CashierDailySummarySnapshot
            {
                Date = dayStart,
                TotalSalesAmount = totalSalesAmount,
                ReceiptCount = receiptCount,
                TotalItemsSold = totalItems,
                AverageReceiptAmount = receiptCount == 0 ? 0m : totalSalesAmount / receiptCount,
                HighestReceiptAmount = highestReceipt,
                Sales = receiptRows
            };
        }

        public void Dispose() => _context.Dispose();
    }

    public sealed class CashierDailySummarySnapshot
    {
        public DateTime Date { get; set; }
        public decimal TotalSalesAmount { get; set; }
        public int ReceiptCount { get; set; }
        public decimal TotalItemsSold { get; set; }
        public decimal AverageReceiptAmount { get; set; }
        public decimal HighestReceiptAmount { get; set; }
        public IReadOnlyList<CashierReceiptSummary> Sales { get; set; } = Array.Empty<CashierReceiptSummary>();
    }

    public sealed class CashierReceiptSummary
    {
        public int SaleId { get; set; }
        public DateTime SaleDateTime { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ItemsCount { get; set; }
    }
}