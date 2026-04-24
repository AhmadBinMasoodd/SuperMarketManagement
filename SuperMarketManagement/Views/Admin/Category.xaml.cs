using System.Windows;
using System.Windows.Controls;
using SuperMarketManagement.Controller;
using CategoryModel = SuperMarketManagement.Models.Category;

namespace SuperMarketManagement.Views.Admin
{
    /// <summary>
    /// Interaction logic for Category.xaml
    /// </summary>
    public partial class Category : UserControl
    {
        private readonly CategoryController _controller = new();
        private CategoryModel? _selectedCategory;
        private DataGrid? CategoryGrid => FindName("CategoryDataGrid") as DataGrid;
        private TextBox? CategoryNameInput => FindName("CategoryNameTextBox") as TextBox;
        private TextBox? CategoryDescriptionInput => FindName("CategoryDescriptionTextBox") as TextBox;
        private CheckBox? IsActiveInput => FindName("IsActiveCheckBox") as CheckBox;
        private TextBox? SearchInput => FindName("SearchTextBox") as TextBox;

        public Category()
        {
            InitializeComponent();
            LoadCategories();
        }

        ~Category()
        {
            _controller.Dispose();
        }

        private void LoadCategories(string? search = null)
        {
            if (CategoryGrid is null)
            {
                return;
            }

            CategoryGrid.ItemsSource = _controller.GetCategories(search);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadCategories(SearchInput?.Text);
        }

        private void CategoryDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryGrid?.SelectedItem is not CategoryModel category)
            {
                return;
            }

            FillForm(category);
        }

        private void FillForm(CategoryModel category)
        {
            if (CategoryNameInput is null || CategoryDescriptionInput is null || IsActiveInput is null)
            {
                return;
            }

            _selectedCategory = category;
            CategoryNameInput.Text = category.Name;
            CategoryDescriptionInput.Text = category.Description ?? string.Empty;
            IsActiveInput.IsChecked = category.IsActive;
        }

        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryNameInput is null || CategoryDescriptionInput is null || IsActiveInput is null)
            {
                return;
            }

            var validation = _controller.Validate(CategoryNameInput.Text, CategoryDescriptionInput.Text);
            if (!validation.IsValid)
            {
                MessageBox.Show(validation.Message, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_controller.ExistsByName(CategoryNameInput.Text))
            {
                MessageBox.Show("Category name already exists.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _controller.AddCategory(CategoryNameInput.Text, CategoryDescriptionInput.Text, IsActiveInput.IsChecked ?? true);
            ClearForm();
            LoadCategories(SearchInput?.Text);
        }

        private void UpdateCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCategory is null)
            {
                MessageBox.Show("Select a category first.", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (CategoryNameInput is null || CategoryDescriptionInput is null || IsActiveInput is null)
            {
                return;
            }

            var validation = _controller.Validate(CategoryNameInput.Text, CategoryDescriptionInput.Text);
            if (!validation.IsValid)
            {
                MessageBox.Show(validation.Message, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_controller.ExistsByName(CategoryNameInput.Text, _selectedCategory.Id))
            {
                MessageBox.Show("Category name already exists.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var updated = _controller.UpdateCategory(_selectedCategory.Id, CategoryNameInput.Text, CategoryDescriptionInput.Text, IsActiveInput.IsChecked ?? true);
            if (!updated)
            {
                MessageBox.Show("Category not found.", "Update", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ClearForm();
            LoadCategories(SearchInput?.Text);
        }

        private void DeleteCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: int id })
            {
                return;
            }

            if (MessageBox.Show("Delete this category?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            _controller.DeleteCategory(id);
            ClearForm();
            LoadCategories(SearchInput?.Text);
        }

        private void ClearForm()
        {
            _selectedCategory = null;
            CategoryNameInput?.Clear();
            CategoryDescriptionInput?.Clear();
            if (IsActiveInput is not null)
            {
                IsActiveInput.IsChecked = true;
            }

            if (CategoryGrid is not null)
            {
                CategoryGrid.SelectedItem = null;
            }
        }
    }
}
