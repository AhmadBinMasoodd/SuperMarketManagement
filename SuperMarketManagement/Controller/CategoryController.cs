using System;
using System.Collections.Generic;
using System.Linq;
using SuperMarketManagement.Models;

namespace SuperMarketManagement.Controller;

public sealed class CategoryController : IDisposable
{
    private readonly MarketDbContext _context = new();

    public List<Category> GetCategories(string? search = null)
    {
        var query = _context.Categories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var text = search.Trim();
            query = query.Where(c => c.Name.Contains(text) || (c.Description != null && c.Description.Contains(text)));
        }

        return query
            .OrderBy(c => c.Name)
            .ToList();
    }

    public Category? GetCategoryById(int id)
    {
        return _context.Categories.FirstOrDefault(c => c.Id == id);
    }

    public (bool IsValid, string Message) Validate(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return (false, "Category name is required.");
        }

        if (name.Trim().Length > 30)
        {
            return (false, "Category name cannot exceed 30 characters.");
        }

        if (!string.IsNullOrWhiteSpace(description) && description.Trim().Length > 50)
        {
            return (false, "Description cannot exceed 50 characters.");
        }

        return (true, string.Empty);
    }

    public bool ExistsByName(string name, int? excludingId = null)
    {
        var normalized = name.Trim();

        return _context.Categories.Any(c =>
            c.Name == normalized &&
            (!excludingId.HasValue || c.Id != excludingId.Value));
    }

    public void AddCategory(string name, string? description, bool isActive)
    {
        var category = new Category
        {
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            IsActive = isActive,
            CreatedAt = DateTime.Now
        };

        _context.Categories.Add(category);
        _context.SaveChanges();
    }

    public bool UpdateCategory(int id, string name, string? description, bool isActive)
    {
        var category = _context.Categories.FirstOrDefault(c => c.Id == id);
        if (category is null)
        {
            return false;
        }

        category.Name = name.Trim();
        category.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        category.IsActive = isActive;
        category.UpdatedAt = DateTime.Now;

        _context.SaveChanges();
        return true;
    }

    public bool DeleteCategory(int id)
    {
        var category = _context.Categories.FirstOrDefault(c => c.Id == id);
        if (category is null)
        {
            return false;
        }

        _context.Categories.Remove(category);
        _context.SaveChanges();
        return true;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
