using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels;

namespace SuperMarketManagement.Services
{
    public sealed class SalesService : IDisposable
    {
        private readonly MarketDbContext _context = new();

        public int CreateSale(int userId, IReadOnlyCollection<BillItem> items)
        {
            if (items.Count == 0)
            {
                throw new InvalidOperationException("No items to bill.");
            }

            var totalAmount = items.Sum(i => i.Total);
            var sale = new Sale
            {
                UserId = userId,
                SaleDateTime = DateTime.Now,
                TotalAmount = totalAmount,
                PaymentMethod = null,
                Remarks = "Sale"
            };

            _context.Sales.Add(sale);
            _context.SaveChanges();

            foreach (var item in items)
            {
                _context.SaleItems.Add(new SaleItem
                {
                    SaleId = sale.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });

                var product = _context.Products.Find(item.ProductId);
                if (product != null)
                {
                    if (product.Quantity < item.Quantity)
                    {
                        throw new InvalidOperationException($"Insufficient stock for {product.Name}.");
                    }

                    product.Quantity -= item.Quantity;
                }

                _context.StockTransactions.Add(new StockTransaction
                {
                    ProductId = item.ProductId,
                    UserId = userId,
                    TransactionType = ResolveTransactionType(isIncrease: false),
                    QuantityChanged = item.Quantity,
                    Remarks = "Stock Out",
                    TransactionDateTime = DateTime.Now
                });
            }

            _context.SaveChanges();
            return sale.Id;
        }

        public IReadOnlyList<SalesProductItem> GetProducts(string? searchText)
        {
            var query = _context.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var text = searchText.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(text));
            }

            return (from p in query
                    join c in _context.Categories.AsNoTracking() on p.CategoryId equals c.Id into categoryJoin
                    from c in categoryJoin.DefaultIfEmpty()
                    orderby p.Name
                    select new SalesProductItem
                    {
                        Id = p.Id,
                        Name = p.Name,
                        CategoryName = c != null ? c.Name : "Unknown",
                        Price = p.SalePrice,
                        Stock = p.Quantity
                    }).ToList();
        }

        private string ResolveTransactionType(bool isIncrease)
        {
            var allowedTypes = GetAllowedTransactionTypes();
            if (allowedTypes.Count == 0)
            {
                return isIncrease ? "In" : "Out";
            }

            var token = isIncrease ? "in" : "out";
            var match = allowedTypes.FirstOrDefault(t => t.Contains(token, StringComparison.OrdinalIgnoreCase));
            return !string.IsNullOrWhiteSpace(match) ? match : allowedTypes.First();
        }

        private IReadOnlyList<string> GetAllowedTransactionTypes()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT definition FROM sys.check_constraints WHERE name = 'CK_StockTransactions_TransactionType'";
                var definition = command.ExecuteScalar() as string;
                if (string.IsNullOrWhiteSpace(definition))
                {
                    return Array.Empty<string>();
                }

                var matches = Regex.Matches(definition, "'([^']+)'", RegexOptions.IgnoreCase);
                return matches.Select(m => m.Groups[1].Value).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        public void Dispose() => _context.Dispose();
    }
}
