using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels;

namespace SuperMarketManagement.Services
{
    public sealed class RefundService : IDisposable
    {
        private readonly MarketDbContext _context = new();

        public RefundViewModel? GetReceiptDetails(int saleId)
        {
            var saleInfo = (from sale in _context.Sales.AsNoTracking()
                            join user in _context.Users.AsNoTracking() on sale.UserId equals user.Id into userJoin
                            from user in userJoin.DefaultIfEmpty()
                            where sale.Id == saleId
                            select new
                            {
                                sale.Id,
                                sale.SaleDateTime,
                                sale.TotalAmount,
                                CashierName = user != null ? user.Name : "Unknown"
                            }).FirstOrDefault();

            if (saleInfo == null)
            {
                return null;
            }

            var items = (from item in _context.SaleItems.AsNoTracking()
                         join product in _context.Products.AsNoTracking() on item.ProductId equals product.Id into productJoin
                         from product in productJoin.DefaultIfEmpty()
                         where item.SaleId == saleId
                         orderby product != null ? product.Name : string.Empty
                         select new RefundLineItemViewModel
                         {
                             ProductName = product != null ? product.Name : "Unknown",
                             Quantity = item.Quantity,
                             UnitPrice = item.UnitPrice,
                             LineTotal = item.LineTotal == 0 ? item.Quantity * item.UnitPrice : item.LineTotal
                         }).ToList();

            for (var i = 0; i < items.Count; i++)
            {
                items[i].SrNo = i + 1;
            }

            return new RefundViewModel
            {
                SaleId = saleInfo.Id,
                SaleDateTime = saleInfo.SaleDateTime,
                TotalAmount = saleInfo.TotalAmount,
                CashierName = saleInfo.CashierName,
                Items = new ObservableCollection<RefundLineItemViewModel>(items)
            };
        }

        public int CreateRefund(SaleRefund refund)
        {
            ArgumentNullException.ThrowIfNull(refund);

            if (refund.SaleId <= 0)
            {
                throw new ArgumentException("SaleId is required.", nameof(refund));
            }

            if (refund.ProductId <= 0)
            {
                throw new ArgumentException("ProductId is required.", nameof(refund));
            }

            if (refund.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than zero.", nameof(refund));
            }

            if (refund.RefundAmount < 0)
            {
                throw new ArgumentException("RefundAmount cannot be negative.", nameof(refund));
            }

            if (!_context.Sales.Any(s => s.Id == refund.SaleId))
            {
                throw new InvalidOperationException($"Sale {refund.SaleId} was not found.");
            }

            if (!_context.Products.Any(p => p.Id == refund.ProductId))
            {
                throw new InvalidOperationException($"Product {refund.ProductId} was not found.");
            }

            if (refund.RefundDateTime == default)
            {
                refund.RefundDateTime = DateTime.Now;
            }

            _context.SaleRefunds.Add(refund);
            _context.SaveChanges();

            return refund.Id;
        }

        public void Dispose() => _context.Dispose();
    }
}
