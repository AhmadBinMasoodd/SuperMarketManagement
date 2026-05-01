using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SuperMarketManagement.Models;
using SuperMarketManagement.Services;
using SuperMarketManagement.ViewModels.Base;
using CategoryModel = SuperMarketManagement.Models.Category;
using Microsoft.EntityFrameworkCore;

namespace SuperMarketManagement.ViewModels
{
    public class ProductGridItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
    }

    public class CategoryOption
    {
        public int Id { get; set; }
        public string Display { get; set; } = string.Empty;
    }

    public class ProductViewModel : ViewModelBase, IDisposable
    {
        private const string StockInRemark = "Stock In";
        private const string StockOutRemark = "Stock Out";
        private const string StockDamagedRemark = "Stock Damaged";
        private readonly ProductService _productService = new();
        private readonly User _currentUser;

        private ObservableCollection<ProductGridItem> _products = new();
        public ObservableCollection<ProductGridItem> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        private ObservableCollection<CategoryOption> _categories = new();
        public ObservableCollection<CategoryOption> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        private CategoryOption? _selectedCategory;
        public CategoryOption? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value) && value != null)
                {
                    CategoryId = value.Id;
                }
            }
        }

        private ProductGridItem? _selectedProduct;
        public ProductGridItem? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value))
                {
                    if (_selectedProduct != null)
                    {
                        var product = _productService.GetProduct(_selectedProduct.Id);
                        if (product != null)
                        {
                            Name = product.Name;
                            CategoryId = product.CategoryId;
                            SelectedCategory = Categories.FirstOrDefault(c => c.Id == product.CategoryId);
                            PurchasePrice = product.PurchasePrice.ToString("0.00");
                            SalePrice = product.SalePrice.ToString("0.00");
                            Quantity = product.Quantity.ToString("0.00");
                            Unit = product.Unit;
                        }
                    }
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

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private int _categoryId;
        public int CategoryId
        {
            get => _categoryId;
            set => SetProperty(ref _categoryId, value);
        }

        private string _purchasePrice = string.Empty;
        public string PurchasePrice
        {
            get => _purchasePrice;
            set => SetProperty(ref _purchasePrice, value);
        }

        private string _salePrice = string.Empty;
        public string SalePrice
        {
            get => _salePrice;
            set => SetProperty(ref _salePrice, value);
        }

        private string _quantity = string.Empty;
        public string Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        private string _unit = string.Empty;
        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        public bool IsAdmin => string.Equals(_currentUser?.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        public bool IsManager => string.Equals(_currentUser?.Role, "Manager", StringComparison.OrdinalIgnoreCase);

        public Visibility AddButtonVisibility => IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DeleteColumnVisibility => IsAdmin || IsManager ? Visibility.Visible : Visibility.Collapsed;
        public bool IsNameReadOnly => IsManager;
        public bool IsCategoryEnabled => IsAdmin || IsManager;
        public bool IsUnitReadOnly => IsManager;

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }

        public ProductViewModel(User user)
        {
            _currentUser = user;

            AddCommand = new RelayCommand(_ => ExecuteAdd(), _ => IsAdmin);
            UpdateCommand = new RelayCommand(_ => ExecuteUpdate(), _ => IsAdmin || IsManager);
            DeleteCommand = new RelayCommand(ExecuteDelete, _ => IsAdmin || IsManager);
            ClearCommand = new RelayCommand(_ => ClearForm());

            LoadCategories();
            LoadProducts();
        }

        private IReadOnlyList<string> GetAllowedTransactionTypes()
        {
            return _productService.GetAllowedTransactionTypes();
        }

        private string ResolveTransactionType(bool isIncrease, bool isInitial)
        {
            var allowedTypes = GetAllowedTransactionTypes();
            if (allowedTypes.Count == 0)
            {
                return isIncrease ? "In" : "Out";
            }

            if (isInitial)
            {
                var initial = allowedTypes.FirstOrDefault(t => t.Contains("initial", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(initial))
                {
                    return initial;
                }
            }

            var token = isIncrease ? "in" : "out";
            var match = allowedTypes.FirstOrDefault(t => t.Contains(token, StringComparison.OrdinalIgnoreCase));
            return !string.IsNullOrWhiteSpace(match) ? match : allowedTypes.First();
        }

        private void LoadCategories()
        {
            try
            {
                Categories = new ObservableCollection<CategoryOption>(_productService.GetCategoryOptions());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}");
            }
        }

        private void LoadProducts()
        {
            Products = new ObservableCollection<ProductGridItem>(_productService.GetProducts(SearchText));
        }

        private (bool IsValid, string Message, decimal Pur, decimal Sal, decimal Qty) Validate()
        {
            if (string.IsNullOrWhiteSpace(Name)) return (false, "Name is required.", 0, 0, 0);
            if (CategoryId <= 0) return (false, "Category is required.", 0, 0, 0);
            if (!decimal.TryParse(PurchasePrice, out decimal p) || p < 0) return (false, "Invalid purchase price.", 0, 0, 0);
            if (!decimal.TryParse(SalePrice, out decimal s) || s < 0) return (false, "Invalid sale price.", 0, 0, 0);
            if (!decimal.TryParse(Quantity, out decimal q) || q < 0) return (false, "Invalid quantity.", 0, 0, 0);
            if (string.IsNullOrWhiteSpace(Unit)) return (false, "Unit is required.", 0, 0, 0);
            return (true, string.Empty, p, s, q);
        }

        private void ExecuteAdd()
        {
            var v = Validate();
            if (!v.IsValid) { MessageBox.Show(v.Message); return; }

            try
            {
                var product = new Product
                {
                    Name = Name.Trim(),
                    CategoryId = CategoryId,
                    PurchasePrice = v.Pur,
                    SalePrice = v.Sal,
                    Quantity = v.Qty,
                    Unit = Unit.Trim()
                };

                _productService.AddProduct(product);

                if (v.Qty > 0)
                {
                    _productService.AddStockTransaction(new StockTransaction
                    {
                        ProductId = product.Id,
                        UserId = _currentUser.Id,
                        TransactionType = ResolveTransactionType(isIncrease: true, isInitial: true),
                        QuantityChanged = v.Qty,
                        Remarks = StockInRemark,
                        TransactionDateTime = DateTime.Now
                    });
                }

                LoadProducts();
                ClearForm();
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show(ex.InnerException?.Message ?? ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ExecuteUpdate()
        {
            if (SelectedProduct == null) return;
            var v = Validate();
            if (!v.IsValid) { MessageBox.Show(v.Message); return; }

            try
            {
                var product = _productService.GetProduct(SelectedProduct.Id);
                if (product == null) return;

                decimal diff = v.Qty - product.Quantity;

                product.Name = Name.Trim();
                product.CategoryId = CategoryId;
                product.PurchasePrice = v.Pur;
                product.SalePrice = v.Sal;
                product.Quantity = v.Qty;
                product.Unit = Unit.Trim();

                if (diff != 0)
                {
                    _productService.AddStockTransaction(new StockTransaction
                    {
                        ProductId = product.Id,
                        UserId = _currentUser.Id,
                        TransactionType = ResolveTransactionType(diff > 0, isInitial: false),
                        QuantityChanged = Math.Abs(diff),
                        Remarks = diff > 0 ? StockInRemark : StockOutRemark,
                        TransactionDateTime = DateTime.Now
                    });
                }

                _productService.UpdateProduct(product);
                LoadProducts();
                ClearForm();
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show(ex.InnerException?.Message ?? ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ExecuteDelete(object? parameter)
        {
            if (parameter is not int id || MessageBox.Show("Delete product?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            try
            {
                _productService.DeleteProduct(id);
                LoadProducts();
                ClearForm();
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show(ex.InnerException?.Message ?? ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ClearForm()
        {
            SelectedProduct = null;
            SelectedCategory = null;
            Name = string.Empty;
            CategoryId = 0;
            PurchasePrice = string.Empty;
            SalePrice = string.Empty;
            Quantity = string.Empty;
            Unit = string.Empty;
        }

        public void Dispose() => _productService.Dispose();
    }
}