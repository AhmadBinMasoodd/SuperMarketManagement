using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SuperMarketManagement.Models;

namespace SuperMarketManagement.Controller;

public sealed class ProductController : IDisposable
{
    private readonly MarketDbContext _context = new();

    public List<Category> GetCategories()
    {
        return _context.Categories
            .OrderBy(c => c.Name)
            .ToList();
    }

    public List<ProductGridItem> GetProducts(string? search = null)
    {
        var query = _context.Products
            .AsNoTracking()
            .Join(
                _context.Categories.AsNoTracking(),
                p => p.CategoryId,
                c => c.Id,
                (p, c) => new ProductGridItem
                {
                    Id = p.Id,
                    Name = p.Name,
                    CategoryId = p.CategoryId,
                    CategoryName = c.Name,
                    PurchasePrice = p.PurchasePrice,
                    SalePrice = p.SalePrice,
                    Quantity = p.Quantity,
                    Unit = p.Unit
                });

        if (!string.IsNullOrWhiteSpace(search))
        {
            var text = search.Trim();
            query = query.Where(x =>
                x.Name.Contains(text) ||
                x.CategoryName.Contains(text) ||
                x.Unit.Contains(text));
        }

        return query
            .OrderBy(x => x.Name)
            .ToList();
    }

    public Product? GetProductById(int id)
    {
        return _context.Products.FirstOrDefault(p => p.Id == id);
    }

    public (bool IsValid, string Message) Validate(ProductInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name) ||
            input.CategoryId <= 0 ||
            string.IsNullOrWhiteSpace(input.PurchasePriceText) ||
            string.IsNullOrWhiteSpace(input.SalePriceText) ||
            string.IsNullOrWhiteSpace(input.QuantityText) ||
            string.IsNullOrWhiteSpace(input.Unit))
        {
            return (false, "Please fill all required fields.");
        }

        if (!decimal.TryParse(input.PurchasePriceText, out var purchasePrice) || purchasePrice <= 0)
        {
            return (false, "Enter a valid purchase price.");
        }

        if (!decimal.TryParse(input.SalePriceText, out var salePrice) || salePrice <= 0)
        {
            return (false, "Enter a valid sale price.");
        }

        if (salePrice <= purchasePrice)
        {
            return (false, "Sale price must be greater than purchase price.");
        }

        if (!decimal.TryParse(input.QuantityText, out var quantity) || quantity < 0)
        {
            return (false, "Enter a valid quantity.");
        }

        if (input.Name.Trim().Length > 30)
        {
            return (false, "Product name cannot exceed 30 characters.");
        }

        if (input.Unit.Trim().Length > 20)
        {
            return (false, "Unit cannot exceed 20 characters.");
        }

        return (true, string.Empty);
    }

    public void AddProduct(ProductInput input)
    {
        var product = new Product
        {
            Name = input.Name.Trim(),
            CategoryId = input.CategoryId,
            PurchasePrice = decimal.Parse(input.PurchasePriceText),
            SalePrice = decimal.Parse(input.SalePriceText),
            Quantity = decimal.Parse(input.QuantityText),
            Unit = input.Unit.Trim()
        };

        _context.Products.Add(product);
        _context.SaveChanges();
    }

    public bool UpdateProduct(int id, ProductInput input)
    {
        var product = _context.Products.FirstOrDefault(p => p.Id == id);
        if (product is null)
        {
            return false;
        }

        product.Name = input.Name.Trim();
        product.CategoryId = input.CategoryId;
        product.PurchasePrice = decimal.Parse(input.PurchasePriceText);
        product.SalePrice = decimal.Parse(input.SalePriceText);
        product.Quantity = decimal.Parse(input.QuantityText);
        product.Unit = input.Unit.Trim();

        _context.SaveChanges();
        return true;
    }

    public bool DeleteProduct(int id)
    {
        var product = _context.Products.FirstOrDefault(p => p.Id == id);
        if (product is null)
        {
            return false;
        }

        _context.Products.Remove(product);
        _context.SaveChanges();
        return true;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

public sealed class ProductInput
{
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string PurchasePriceText { get; set; } = string.Empty;
    public string SalePriceText { get; set; } = string.Empty;
    public string QuantityText { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
}

public sealed class ProductGridItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
}
