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
    public class SalesProductItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Stock { get; set; }
    }

    public class BillItem : ViewModelBase
    {
        private int _srNo;
        private string _name = string.Empty;
        private int _quantity;
        private decimal _unitPrice;

        public int ProductId { get; set; }

        public int SrNo
        {
            get => _srNo;
            set => SetProperty(ref _srNo, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (SetProperty(ref _quantity, value))
                {
                    OnPropertyChanged(nameof(Total));
                }
            }
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (SetProperty(ref _unitPrice, value))
                {
                    OnPropertyChanged(nameof(Total));
                }
            }
        }

        public decimal Total => UnitPrice * Quantity;
    }

    public class SalesViewModel : ViewModelBase, IDisposable
    {
        private readonly SalesService _salesService = new();
        private readonly User _currentUser;

        private ObservableCollection<SalesProductItem> _products = new();
        public ObservableCollection<SalesProductItem> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        private ObservableCollection<BillItem> _billItems = new();
        public ObservableCollection<BillItem> BillItems
        {
            get => _billItems;
            set => SetProperty(ref _billItems, value);
        }

        private SalesProductItem? _selectedProduct;
        public SalesProductItem? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value) && value != null)
                {
                    AddToBill(value, fromSelection: true);
                    ClearSelection();
                }
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    LoadProducts();
                }
            }
        }

        private decimal _grandTotal;
        public decimal GrandTotal
        {
            get => _grandTotal;
            set => SetProperty(ref _grandTotal, value);
        }

        public ICommand AddToBillCommand { get; }
        public ICommand GenerateBillCommand { get; }

        private int? _lastAutoAddProductId;
        private DateTime _lastAutoAddTime;

        public SalesViewModel(User user)
        {
            _currentUser = user;
            AddToBillCommand = new RelayCommand(ExecuteAddToBill);
            GenerateBillCommand = new RelayCommand(_ => ExecuteGenerateBill());
            LoadProducts();
        }

        private void LoadProducts()
        {
            Products = new ObservableCollection<SalesProductItem>(_salesService.GetProducts(SearchText));
        }

        private void ExecuteAddToBill(object? parameter)
        {
            if (parameter is not SalesProductItem item)
            {
                return;
            }

            var now = DateTime.UtcNow;
            if (_lastAutoAddProductId == item.Id && (now - _lastAutoAddTime).TotalMilliseconds < 400)
            {
                return;
            }

            AddToBill(item, fromSelection: false);
            ClearSelection();
        }

        private void AddToBill(SalesProductItem item, bool fromSelection)
        {
            if (fromSelection)
            {
                _lastAutoAddProductId = item.Id;
                _lastAutoAddTime = DateTime.UtcNow;
            }

            var existing = BillItems.FirstOrDefault(b => b.ProductId == item.Id);
            if (existing != null)
            {
                existing.Quantity += 1;
            }
            else
            {
                BillItems.Add(new BillItem
                {
                    ProductId = item.Id,
                    Name = item.Name,
                    Quantity = 1,
                    UnitPrice = item.Price
                });
            }

            RecalculateTotals();
        }

        private void RecalculateTotals()
        {
            for (var i = 0; i < BillItems.Count; i++)
            {
                BillItems[i].SrNo = i + 1;
            }

            GrandTotal = BillItems.Sum(b => b.Total);
            OnPropertyChanged(nameof(BillItems));
        }

        private void ClearSelection()
        {
            _selectedProduct = null;
            OnPropertyChanged(nameof(SelectedProduct));
        }

        private void ExecuteGenerateBill()
        {
            try
            {
                var billSnapshot = BillItems.Select(item => new BillItem
                {
                    ProductId = item.ProductId,
                    SrNo = item.SrNo,
                    Name = item.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList();

                var saleId = _salesService.CreateSale(_currentUser.Id, billSnapshot);
                PrintBill(saleId, billSnapshot, GrandTotal);

                MessageBox.Show($"Sale #{saleId} saved successfully.");
                BillItems.Clear();
                GrandTotal = 0;
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PrintBill(int saleId, IReadOnlyCollection<BillItem> items, decimal total)
        {
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
                PagePadding = new Thickness(24)
            };
            document.ColumnWidth = dialog.PrintableAreaWidth;

            document.Blocks.Add(new Paragraph(new Run("Super Market"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 2)
            });

            document.Blocks.Add(new Paragraph(new Run("Sales Receipt"))
            {
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });

            document.Blocks.Add(new Paragraph(new Run($"Receipt #: {saleId}")) { Margin = new Thickness(0, 0, 0, 2) });
            document.Blocks.Add(new Paragraph(new Run($"Cashier: {_currentUser.Name}")) { Margin = new Thickness(0, 0, 0, 2) });
            document.Blocks.Add(new Paragraph(new Run($"Date: {DateTime.Now:g}")) { Margin = new Thickness(0, 0, 0, 8) });

            document.Blocks.Add(new Paragraph(new Run(new string('-', 48)))
            {
                FontSize = 11,
                Foreground = System.Windows.Media.Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 6)
            });

            var table = new Table
            {
                CellSpacing = 6
            };
            table.Columns.Add(new TableColumn { Width = new GridLength(0.6, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(3.2, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(0.9, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(1.2, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(1.3, GridUnitType.Star) });

            var header = new TableRowGroup();
            var headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("#")) { TextAlignment = TextAlignment.Left }) { FontWeight = FontWeights.Bold });
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Item")) { TextAlignment = TextAlignment.Left }) { FontWeight = FontWeights.Bold });
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Qty")) { TextAlignment = TextAlignment.Right }) { FontWeight = FontWeights.Bold });
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Unit (PKR)")) { TextAlignment = TextAlignment.Right }) { FontWeight = FontWeights.Bold });
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Total (PKR)")) { TextAlignment = TextAlignment.Right }) { FontWeight = FontWeights.Bold });
            header.Rows.Add(headerRow);
            table.RowGroups.Add(header);

            var rows = new TableRowGroup();
            foreach (var item in items)
            {
                var row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.SrNo.ToString())) { TextAlignment = TextAlignment.Left }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.Name)) { TextAlignment = TextAlignment.Left }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString("N0")))
                {
                    TextAlignment = TextAlignment.Right
                }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.UnitPrice.ToString("N2")))
                {
                    TextAlignment = TextAlignment.Right
                }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.Total.ToString("N2")))
                {
                    TextAlignment = TextAlignment.Right
                }));
                rows.Rows.Add(row);
            }
            table.RowGroups.Add(rows);

            document.Blocks.Add(table);
            document.Blocks.Add(new Paragraph(new Run($"Grand Total (PKR): {total:N2}"))
            {
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 12, 0, 0)
            });

            dialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, $"Sale {saleId}");
        }

        public void Dispose() => _salesService.Dispose();
    }
}
