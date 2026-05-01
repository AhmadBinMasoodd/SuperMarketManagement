using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels;

namespace SuperMarketManagement.Services
{
    public sealed class ProductService : IDisposable
    {
        private readonly MarketDbContext _context = new();

        public IReadOnlyList<CategoryOption> GetCategoryOptions()
        {
            return _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CategoryOption
                {
                    Id = c.Id,
                    Display = $"{c.Id} - {c.Name}"
                })
                .ToList();
        }

        public IReadOnlyList<ProductGridItem> GetProducts(string? searchText)
        {
            var query = _context.Products.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var text = searchText.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(text));
            }

            return (from p in query
                    join c in _context.Categories on p.CategoryId equals c.Id into categoryJoin
                    from c in categoryJoin.DefaultIfEmpty()
                    orderby p.Name
                    select new ProductGridItem
                    {
                        Id = p.Id,
                        Name = p.Name,
                        CategoryName = c != null ? c.Name : "Unknown",
                        Quantity = p.Quantity,
                        Unit = p.Unit,
                        PurchasePrice = p.PurchasePrice,
                        SalePrice = p.SalePrice
                    }).ToList();
        }

        public Product? GetProduct(int id) => _context.Products.Find(id);

        public void AddProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        public void UpdateProduct(Product product)
        {
            _context.Products.Update(product);
            _context.SaveChanges();
        }

        public void AddStockTransaction(StockTransaction transaction)
        {
            _context.StockTransactions.Add(transaction);
            _context.SaveChanges();
        }

        public IReadOnlyList<string> GetAllowedTransactionTypes()
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

        public void DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
        }

        public void Dispose() => _context.Dispose();
    }
}
