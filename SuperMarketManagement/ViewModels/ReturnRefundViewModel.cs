using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using SuperMarketManagement.Models;
using SuperMarketManagement.Services;
using SuperMarketManagement.ViewModels.Base;

namespace SuperMarketManagement.ViewModels
{
    public sealed class ReturnRefundViewModel : ViewModelBase, IDisposable
    {
        private readonly RefundService _refundService = new();
        private readonly User _currentUser;

        private string _receiptNumberText = string.Empty;
        public string ReceiptNumberText
        {
            get => _receiptNumberText;
            set => SetProperty(ref _receiptNumberText, value);
        }

        private int _saleId;
        public int SaleId
        {
            get => _saleId;
            set => SetProperty(ref _saleId, value);
        }

        private string _cashierName = string.Empty;
        public string CashierName
        {
            get => _cashierName;
            set => SetProperty(ref _cashierName, value);
        }

        private DateTime _saleDateTime;
        public DateTime SaleDateTime
        {
            get => _saleDateTime;
            set => SetProperty(ref _saleDateTime, value);
        }

        private decimal _originalTotalAmount;
        public decimal OriginalTotalAmount
        {
            get => _originalTotalAmount;
            set => SetProperty(ref _originalTotalAmount, value);
        }

        private decimal _updatedTotalAmount;
        public decimal UpdatedTotalAmount
        {
            get => _updatedTotalAmount;
            set => SetProperty(ref _updatedTotalAmount, value);
        }

        private ObservableCollection<EditableReceiptLineItemViewModel> _items = new();
        public ObservableCollection<EditableReceiptLineItemViewModel> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        private bool _isReceiptLoaded;
        public bool IsReceiptLoaded
        {
            get => _isReceiptLoaded;
            set
            {
                if (SetProperty(ref _isReceiptLoaded, value))
                {
                    OnPropertyChanged(nameof(ReceiptSectionVisibility));
                }
            }
        }

        public Visibility ReceiptSectionVisibility => IsReceiptLoaded ? Visibility.Visible : Visibility.Collapsed;

        private string _statusMessage = "Search a receipt number to load details.";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand SearchReceiptCommand { get; }
        public ICommand IncreaseQtyCommand { get; }
        public ICommand DecreaseQtyCommand { get; }
        public ICommand ApplyAndPrintCommand { get; }

        public ReturnRefundViewModel(User user)
        {
            _currentUser = user;

            SearchReceiptCommand = new RelayCommand(_ => ExecuteSearchReceipt());
            IncreaseQtyCommand = new RelayCommand(ExecuteIncreaseQty);
            DecreaseQtyCommand = new RelayCommand(ExecuteDecreaseQty);
            ApplyAndPrintCommand = new RelayCommand(_ => ExecuteApplyAndPrint(), _ => IsReceiptLoaded);
        }

        private void ExecuteSearchReceipt()
        {
            if (!int.TryParse(ReceiptNumberText, out var saleId) || saleId <= 0)
            {
                MessageBox.Show("Please enter a valid receipt number.", "Return & Refund", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var receipt = _refundService.GetEditableReceiptDetails(saleId);
            if (receipt == null)
            {
                MessageBox.Show($"Receipt {saleId} was not found.", "Return & Refund", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            LoadReceipt(receipt);
            StatusMessage = $"Receipt {receipt.SaleId} loaded. Use +/- to adjust quantities.";
        }

        private void ExecuteIncreaseQty(object? parameter)
        {
            if (parameter is not EditableReceiptLineItemViewModel item)
            {
                return;
            }

            item.CurrentQuantity += 1;
            RecalculateTotals();
        }

        private void ExecuteDecreaseQty(object? parameter)
        {
            if (parameter is not EditableReceiptLineItemViewModel item)
            {
                return;
            }

            if (item.CurrentQuantity <= 0)
            {
                return;
            }

            item.CurrentQuantity -= 1;
            RecalculateTotals();
        }

        private void ExecuteApplyAndPrint()
        {
            if (!IsReceiptLoaded)
            {
                return;
            }

            try
            {
                var adjustments = Items.Select(i => new ReceiptQuantityAdjustment
                {
                    ProductId = i.ProductId,
                    NewQuantity = i.CurrentQuantity
                }).ToList();

                var result = _refundService.ApplyReceiptAdjustments(_currentUser.Id, SaleId, adjustments);
                LoadReceipt(result.UpdatedReceipt);

                PrintUpdatedBill();

                StatusMessage = result.HasChanges
                    ? "Changes saved and updated receipt printed successfully."
                    : "No quantity changes detected. Current receipt printed.";

                MessageBox.Show(StatusMessage, "Return & Refund", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Return & Refund", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadReceipt(EditableReceiptViewModel receipt)
        {
            SaleId = receipt.SaleId;
            CashierName = receipt.CashierName;
            SaleDateTime = receipt.SaleDateTime;
            OriginalTotalAmount = receipt.OriginalTotalAmount;
            UpdatedTotalAmount = receipt.CurrentTotalAmount;

            Items = new ObservableCollection<EditableReceiptLineItemViewModel>(receipt.Items.Select(item => new EditableReceiptLineItemViewModel
            {
                SrNo = item.SrNo,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                OriginalQuantity = item.OriginalQuantity,
                CurrentQuantity = item.CurrentQuantity,
                UnitPrice = item.UnitPrice
            }));

            IsReceiptLoaded = true;
        }

        private void RecalculateTotals()
        {
            UpdatedTotalAmount = Items.Sum(i => i.LineTotal);
        }

        private void PrintUpdatedBill()
        {
            var printableItems = Items.Where(i => i.CurrentQuantity > 0).ToList();

            var dialog = new PrintDialog();
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var document = new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 11,
                PageWidth = dialog.PrintableAreaWidth,
                PageHeight = dialog.PrintableAreaHeight,
                PagePadding = new Thickness(24),
                ColumnWidth = dialog.PrintableAreaWidth
            };

            document.Blocks.Add(new Paragraph(new Run("Super Market"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 2)
            });

            document.Blocks.Add(new Paragraph(new Run("Updated Return/Refund Receipt"))
            {
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });

            document.Blocks.Add(new Paragraph(new Run($"Receipt #: {SaleId}")) { Margin = new Thickness(0, 0, 0, 2) });
            document.Blocks.Add(new Paragraph(new Run($"Cashier: {CashierName}")) { Margin = new Thickness(0, 0, 0, 2) });
            document.Blocks.Add(new Paragraph(new Run($"Printed By: {_currentUser.Name}")) { Margin = new Thickness(0, 0, 0, 2) });
            document.Blocks.Add(new Paragraph(new Run($"Date: {DateTime.Now:g}")) { Margin = new Thickness(0, 0, 0, 8) });

            var table = new Table { CellSpacing = 6 };
            table.Columns.Add(new TableColumn { Width = new GridLength(0.6, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(3.2, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(0.9, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(1.2, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(1.3, GridUnitType.Star) });

            var header = new TableRowGroup();
            var headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("#"))) { FontWeight = FontWeights.Bold });
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Item"))) { FontWeight = FontWeights.Bold });
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Qty")) { TextAlignment = TextAlignment.Right }) { FontWeight = FontWeights.Bold });
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Unit (PKR)")) { TextAlignment = TextAlignment.Right }) { FontWeight = FontWeights.Bold });
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Total (PKR)")) { TextAlignment = TextAlignment.Right }) { FontWeight = FontWeights.Bold });
            header.Rows.Add(headerRow);
            table.RowGroups.Add(header);

            var rows = new TableRowGroup();
            for (var i = 0; i < printableItems.Count; i++)
            {
                var item = printableItems[i];
                var row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run((i + 1).ToString()))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.ProductName))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.CurrentQuantity.ToString("N0"))) { TextAlignment = TextAlignment.Right }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.UnitPrice.ToString("N2"))) { TextAlignment = TextAlignment.Right }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.LineTotal.ToString("N2"))) { TextAlignment = TextAlignment.Right }));
                rows.Rows.Add(row);
            }

            table.RowGroups.Add(rows);
            document.Blocks.Add(table);

            document.Blocks.Add(new Paragraph(new Run($"Original Total (PKR): {OriginalTotalAmount:N2}"))
            {
                Margin = new Thickness(0, 10, 0, 0)
            });

            document.Blocks.Add(new Paragraph(new Run($"Updated Total (PKR): {UpdatedTotalAmount:N2}"))
            {
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 2, 0, 0)
            });

            dialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, $"Updated Receipt {SaleId}");
        }

        public void Dispose() => _refundService.Dispose();
    }
}