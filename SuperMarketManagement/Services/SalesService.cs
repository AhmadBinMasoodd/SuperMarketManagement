using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels;

namespace SuperMarketManagement.Services
{
    public sealed class SalesService : IDisposable
    {
        private readonly MarketDbContext _context = new();

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

        public void Dispose() => _context.Dispose();
    }
}
