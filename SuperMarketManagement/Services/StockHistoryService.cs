using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels;

namespace SuperMarketManagement.Services
{
    public sealed class StockHistoryService : IDisposable
    {
        private readonly MarketDbContext _context = new();

        public IReadOnlyList<StockHistoryGridItemViewModel> GetHistory(string? searchText)
        {
            var query = _context.StockTransactions
                .AsNoTracking()
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

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var text = searchText.Trim().ToLower();
                query = query.Where(x => x.ProductName.ToLower().Contains(text) || x.UserName.ToLower().Contains(text) || x.TransactionType.ToLower().Contains(text));
            }

            return query.ToList();
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

        public void Dispose() => _context.Dispose();
    }
}
