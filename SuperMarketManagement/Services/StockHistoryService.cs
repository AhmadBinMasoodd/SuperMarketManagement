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
            var query = from st in _context.StockTransactions.AsNoTracking()
                        join p in _context.Products.AsNoTracking() on st.ProductId equals p.Id into productJoin
                        from p in productJoin.DefaultIfEmpty()
                        join u in _context.Users.AsNoTracking() on st.UserId equals u.Id into userJoin
                        from u in userJoin.DefaultIfEmpty()
                        orderby st.TransactionDateTime descending
                        select new StockHistoryGridItemViewModel
                        {
                            TransactionDateTime = st.TransactionDateTime,
                            ProductName = p != null ? p.Name : "Unknown",
                            UserName = u != null ? u.Name : "Unknown",
                            TransactionType = st.TransactionType,
                            QuantityChanged = st.QuantityChanged,
                            Remarks = st.Remarks
                        };

            var items = query.ToList();

            foreach (var item in items)
            {
                item.TransactionType = NormalizeTransactionType(item.TransactionType);
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var text = searchText.Trim().ToLower();
                items = items
                    .Where(x => x.ProductName.ToLower().Contains(text)
                        || x.UserName.ToLower().Contains(text)
                        || x.TransactionType.ToLower().Contains(text))
                    .ToList();
            }

            return items;
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
