using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels;

namespace SuperMarketManagement.Services
{
    public sealed class RefundService : IDisposable
    {
        private readonly MarketDbContext _context = new();

        public EditableReceiptViewModel? GetEditableReceiptDetails(int saleId)
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
                         join product in _context.Products.AsNoTracking() on item.ProductId equals product.Id
                         where item.SaleId == saleId
                         orderby product.Name
                         select new EditableReceiptLineItemViewModel
                         {
                             ProductId = product.Id,
                             ProductName = product.Name,
                             OriginalQuantity = item.Quantity,
                             CurrentQuantity = item.Quantity,
                             UnitPrice = item.UnitPrice
                         }).ToList();

            for (var i = 0; i < items.Count; i++)
            {
                items[i].SrNo = i + 1;
            }

            return new EditableReceiptViewModel
            {
                SaleId = saleInfo.Id,
                SaleDateTime = saleInfo.SaleDateTime,
                CashierName = saleInfo.CashierName,
                OriginalTotalAmount = saleInfo.TotalAmount,
                CurrentTotalAmount = items.Sum(i => i.LineTotal),
                Items = new ObservableCollection<EditableReceiptLineItemViewModel>(items)
            };
        }

        public EditableReceiptViewModel? GetReceiptDetails(int saleId)
        {
            return GetEditableReceiptDetails(saleId);
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

        public ReceiptAdjustmentResult ApplyReceiptAdjustments(int cashierUserId, int saleId, IReadOnlyCollection<ReceiptQuantityAdjustment> adjustments)
        {
            ArgumentNullException.ThrowIfNull(adjustments);

            var sale = _context.Sales.FirstOrDefault(s => s.Id == saleId)
                ?? throw new InvalidOperationException($"Receipt {saleId} was not found.");

            var dbItems = _context.SaleItems.Where(i => i.SaleId == saleId).ToList();
            if (dbItems.Count == 0)
            {
                throw new InvalidOperationException("No receipt items were found.");
            }

            var adjustmentMap = adjustments.ToDictionary(a => a.ProductId);
            var trackedProducts = _context.Products.Where(p => dbItems.Select(i => i.ProductId).Contains(p.Id)).ToDictionary(p => p.Id);

            using var transaction = _context.Database.BeginTransaction();

            var hasChanges = false;

            foreach (var item in dbItems)
            {
                if (!adjustmentMap.TryGetValue(item.ProductId, out var adj))
                {
                    continue;
                }

                var newQty = adj.NewQuantity;
                if (newQty < 0)
                {
                    throw new InvalidOperationException("Quantity cannot be negative.");
                }

                var oldQty = item.Quantity;
                if (newQty == oldQty)
                {
                    continue;
                }

                hasChanges = true;
                var delta = newQty - oldQty;
                var product = trackedProducts[item.ProductId];

                if (delta > 0)
                {
                    if (product.Quantity < delta)
                    {
                        throw new InvalidOperationException($"Insufficient stock to increase '{product.Name}'. Available: {product.Quantity:N2}.");
                    }

                    product.Quantity -= delta;
                    _context.StockTransactions.Add(new StockTransaction
                    {
                        ProductId = item.ProductId,
                        UserId = cashierUserId,
                        TransactionType = ResolveTransactionType(isIncrease: false),
                        QuantityChanged = delta,
                        Remarks = $"Receipt {saleId} adjustment (Qty +)",
                        TransactionDateTime = DateTime.Now
                    });
                }
                else
                {
                    var returnedQty = Math.Abs(delta);
                    product.Quantity += returnedQty;

                    _context.SaleRefunds.Add(new SaleRefund
                    {
                        SaleId = saleId,
                        ProductId = item.ProductId,
                        Quantity = returnedQty,
                        RefundAmount = returnedQty * item.UnitPrice,
                        RefundDateTime = DateTime.Now,
                        Remarks = "Return/Refund from receipt adjustment"
                    });

                    _context.StockTransactions.Add(new StockTransaction
                    {
                        ProductId = item.ProductId,
                        UserId = cashierUserId,
                        TransactionType = ResolveTransactionType(isIncrease: true),
                        QuantityChanged = returnedQty,
                        Remarks = $"Receipt {saleId} adjustment (Qty -)",
                        TransactionDateTime = DateTime.Now
                    });
                }

                if (newQty == 0)
                {
                    _context.SaleItems.Remove(item);
                }
                else
                {
                    item.Quantity = newQty;
                }
            }

            var remainingItems = _context.SaleItems.Where(i => i.SaleId == saleId).ToList();
            sale.TotalAmount = remainingItems.Sum(i => i.Quantity * i.UnitPrice);

            _context.SaveChanges();
            transaction.Commit();

            var refreshed = GetEditableReceiptDetails(saleId)
                ?? throw new InvalidOperationException("Failed to reload updated receipt.");

            return new ReceiptAdjustmentResult
            {
                HasChanges = hasChanges,
                UpdatedReceipt = refreshed
            };
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

    public sealed class ReceiptQuantityAdjustment
    {
        public int ProductId { get; set; }
        public decimal NewQuantity { get; set; }
    }

    public sealed class ReceiptAdjustmentResult
    {
        public bool HasChanges { get; set; }
        public EditableReceiptViewModel UpdatedReceipt { get; set; } = new();
    }
}
