using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SuperMarketManagement.Models;
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
        public decimal SalePrice { get; set; }
    }

    public class CategoryOption
    {
        public int Id { get; set; }
        public string Display { get; set; } = string.Empty;
    }

    public class ProductViewModel : ViewModelBase, IDisposable
    {
        private readonly MarketDbContext _context = new();
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
                        var product = _context.Products.Find(_selectedProduct.Id);
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
        public Visibility DeleteColumnVisibility => IsAdmin ? Visibility.Visible : Visibility.Collapsed;
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
            UpdateCommand = new RelayCommand(_ => ExecuteUpdate(), _ => IsAdmin);
            DeleteCommand = new RelayCommand(ExecuteDelete, _ => IsAdmin);
            ClearCommand = new RelayCommand(_ => ClearForm());

            LoadCategories();
            LoadProducts();
        }

        private void LoadCategories()
        {
            try
            {
                var categoryList = _context.Categories
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .Select(c => new CategoryOption
                    {
                        Id = c.Id,
                        Display = $"{c.Id} - {c.Name}"
                    })
                    .ToList();
                Categories = new ObservableCollection<CategoryOption>(categoryList);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}");
            }
        }

        private void LoadProducts()
        {
            var query = _context.Products.AsQueryable();
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var text = SearchText.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(text));
            }

            var items = (from p in query
                         join c in _context.Categories on p.CategoryId equals c.Id into categoryJoin
                         from c in categoryJoin.DefaultIfEmpty()
                         orderby p.Name
                         select new ProductGridItem
                         {
                             Id = p.Id,
                             Name = p.Name,
                             CategoryName = c != null ? c.Name : "Unknown",
                             Quantity = p.Quantity,
                             Unit = p.Unit,
                             SalePrice = p.SalePrice
                         }).ToList();

            Products = new ObservableCollection<ProductGridItem>(items);
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

            var product = new Product
            {
                Name = Name.Trim(),
                CategoryId = CategoryId,
                PurchasePrice = v.Pur,
                SalePrice = v.Sal,
                Quantity = v.Qty,
                Unit = Unit.Trim()
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            if (v.Qty > 0)
            {
                _context.StockTransactions.Add(new StockTransaction
                {
                    ProductId = product.Id,
                    UserId = _currentUser.Id,
                    TransactionType = "In",
                    QuantityChanged = v.Qty,
                    Remarks = "Initial Stock",
                    TransactionDateTime = DateTime.Now
                });
                _context.SaveChanges();
            }

            LoadProducts();
            ClearForm();
        }

        private void ExecuteUpdate()
        {
            if (SelectedProduct == null) return;
            var v = Validate();
            if (!v.IsValid) { MessageBox.Show(v.Message); return; }

            var product = _context.Products.Find(SelectedProduct.Id);
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
                _context.StockTransactions.Add(new StockTransaction
                {
                    ProductId = product.Id,
                    UserId = _currentUser.Id,
                    TransactionType = diff > 0 ? "Adjustment-In" : "Adjustment-Out",
                    QuantityChanged = Math.Abs(diff),
                    Remarks = "Manual Adjustment",
                    TransactionDateTime = DateTime.Now
                });
            }

            _context.SaveChanges();
            LoadProducts();
            ClearForm();
        }

        private void ExecuteDelete(object? parameter)
        {
            if (parameter is not int id || MessageBox.Show("Delete product?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
                LoadProducts();
                ClearForm();
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

        public void Dispose() => _context.Dispose();
    }
}