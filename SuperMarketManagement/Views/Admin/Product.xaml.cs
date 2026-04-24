using System.Windows;
using System.Windows.Controls;
using SuperMarketManagement.Controller;
using SuperMarketManagement.Models;

namespace SuperMarketManagement.Views.Admin
{
    public partial class Product : UserControl
    {
        private readonly ProductController _controller = new();
        private Models.Product? _selectedProduct;

        private DataGrid? ProductGrid => FindName("ProductDataGrid") as DataGrid;
        private TextBox? ProductNameInput => FindName("ProductNameTextBox") as TextBox;
        private ComboBox? CategoryInput => FindName("CategoryComboBox") as ComboBox;
        private TextBox? PurchasePriceInput => FindName("PurchasePriceTextBox") as TextBox;
        private TextBox? SalePriceInput => FindName("SalePriceTextBox") as TextBox;
        private TextBox? QuantityInput => FindName("QuantityTextBox") as TextBox;
        private TextBox? UnitInput => FindName("UnitTextBox") as TextBox;
        private TextBox? SearchInput => FindName("SearchTextBox") as TextBox;

        public Product()
        {
            InitializeComponent();
            LoadCategories();
            LoadProducts();
        }

        ~Product()
        {
            _controller.Dispose();
        }

        private void LoadCategories()
        {
            if (CategoryInput is null)
            {
                return;
            }

            CategoryInput.ItemsSource = _controller.GetCategories();
            CategoryInput.SelectedIndex = -1;
        }

        private void LoadProducts(string? search = null)
        {
            if (ProductGrid is null)
            {
                return;
            }

            ProductGrid.ItemsSource = _controller.GetProducts(search);
        }

        private ProductInput? BuildInput()
        {
            if (ProductNameInput is null ||
                CategoryInput is null ||
                PurchasePriceInput is null ||
                SalePriceInput is null ||
                QuantityInput is null ||
                UnitInput is null)
            {
                return null;
            }

            return new ProductInput
            {
                Name = ProductNameInput.Text.Trim(),
                CategoryId = CategoryInput.SelectedValue is int id ? id : 0,
                PurchasePriceText = PurchasePriceInput.Text.Trim(),
                SalePriceText = SalePriceInput.Text.Trim(),
                QuantityText = QuantityInput.Text.Trim(),
                Unit = UnitInput.Text.Trim()
            };
        }

        private void FillForm(Models.Product product)
        {
            if (ProductNameInput is null ||
                CategoryInput is null ||
                PurchasePriceInput is null ||
                SalePriceInput is null ||
                QuantityInput is null ||
                UnitInput is null)
            {
                return;
            }

            _selectedProduct = product;
            ProductNameInput.Text = product.Name;
            CategoryInput.SelectedValue = product.CategoryId;
            PurchasePriceInput.Text = product.PurchasePrice.ToString("0.00");
            SalePriceInput.Text = product.SalePrice.ToString("0.00");
            QuantityInput.Text = product.Quantity.ToString("0.00");
            UnitInput.Text = product.Unit;
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            var input = BuildInput();
            if (input is null)
            {
                return;
            }

            var validation = _controller.Validate(input);
            if (!validation.IsValid)
            {
                MessageBox.Show(validation.Message, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _controller.AddProduct(input);
            LoadProducts(SearchInput?.Text);
            ClearForm();
        }

        private void UpdateProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProduct is null)
            {
                MessageBox.Show("Select a product first.", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var input = BuildInput();
            if (input is null)
            {
                return;
            }

            var validation = _controller.Validate(input);
            if (!validation.IsValid)
            {
                MessageBox.Show(validation.Message, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var updated = _controller.UpdateProduct(_selectedProduct.Id, input);
            if (!updated)
            {
                MessageBox.Show("Product not found.", "Update", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LoadProducts(SearchInput?.Text);
            ClearForm();
        }

        private void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: int id })
            {
                return;
            }

            if (MessageBox.Show("Delete this product?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            _controller.DeleteProduct(id);
            LoadProducts(SearchInput?.Text);
            ClearForm();
        }

        private void ProductDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductGrid?.SelectedItem is not ProductGridItem row)
            {
                return;
            }

            var product = _controller.GetProductById(row.Id);
            if (product is null)
            {
                return;
            }

            FillForm(product);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadProducts(SearchInput?.Text);
        }

        private void ClearForm()
        {
            _selectedProduct = null;
            ProductNameInput?.Clear();
            PurchasePriceInput?.Clear();
            SalePriceInput?.Clear();
            QuantityInput?.Clear();
            UnitInput?.Clear();

            if (CategoryInput is not null)
            {
                CategoryInput.SelectedIndex = -1;
            }

            if (ProductGrid is not null)
            {
                ProductGrid.SelectedItem = null;
            }
        }
    }
}
