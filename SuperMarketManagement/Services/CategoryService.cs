using System;
using System.Collections.Generic;
using System.Linq;
using SuperMarketManagement.Models;

namespace SuperMarketManagement.Services
{
    public sealed class CategoryService : IDisposable
    {
        private readonly MarketDbContext _context = new();

        public IReadOnlyList<Category> GetCategories(string? searchText)
        {
            var query = _context.Categories.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var text = searchText.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(text) || (c.Description != null && c.Description.ToLower().Contains(text)));
            }

            return query.OrderBy(c => c.Name).ToList();
        }

        public bool CategoryNameExists(string name, int? excludeId = null)
        {
            var query = _context.Categories.AsQueryable();
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return query.Any(c => c.Name == name);
        }

        public void AddCategory(Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
        }

        public void UpdateCategory(Category category)
        {
            _context.Categories.Update(category);
            _context.SaveChanges();
        }

        public void DeleteCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
            }
        }

        public void Dispose() => _context.Dispose();
    }
}
