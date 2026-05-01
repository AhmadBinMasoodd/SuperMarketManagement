using System;
using System.Collections.ObjectModel;
using System.Linq;
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

        private int? _lastAutoAddProductId;
        private DateTime _lastAutoAddTime;

        public SalesViewModel(User user)
        {
            _currentUser = user;
            AddToBillCommand = new RelayCommand(ExecuteAddToBill);
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

        public void Dispose() => _salesService.Dispose();
    }
}
