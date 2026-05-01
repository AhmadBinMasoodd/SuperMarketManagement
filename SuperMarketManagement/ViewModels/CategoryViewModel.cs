using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SuperMarketManagement.Models;
using SuperMarketManagement.Services;
using SuperMarketManagement.ViewModels.Base;
using CategoryModel = SuperMarketManagement.Models.Category;

namespace SuperMarketManagement.ViewModels
{
    public class CategoryViewModel : ViewModelBase, IDisposable
    {
        private readonly CategoryService _categoryService = new();

        private ObservableCollection<CategoryModel> _categories = new();
        public ObservableCollection<CategoryModel> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        private CategoryModel? _selectedCategory;
        public CategoryModel? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    if (_selectedCategory != null)
                    {
                        Name = _selectedCategory.Name;
                        Description = _selectedCategory.Description ?? string.Empty;
                        IsActive = _selectedCategory.IsActive;
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
                    LoadCategories();
                }
            }
        }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private bool _isActive = true;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }

        public CategoryViewModel()
        {
            AddCommand = new RelayCommand(_ => ExecuteAdd());
            UpdateCommand = new RelayCommand(_ => ExecuteUpdate());
            DeleteCommand = new RelayCommand(ExecuteDelete);
            ClearCommand = new RelayCommand(_ => ClearForm());

            LoadCategories();
        }

        private void LoadCategories()
        {
            Categories = new ObservableCollection<CategoryModel>(_categoryService.GetCategories(SearchText));
        }

        private (bool IsValid, string Message) Validate()
        {
            if (string.IsNullOrWhiteSpace(Name)) return (false, "Category name is required.");
            if (Name.Trim().Length > 30) return (false, "Category name cannot exceed 30 characters.");
            if (!string.IsNullOrWhiteSpace(Description) && Description.Trim().Length > 50) return (false, "Description cannot exceed 50 characters.");
            return (true, string.Empty);
        }

        private void ExecuteAdd()
        {
            var validation = Validate();
            if (!validation.IsValid)
            {
                MessageBox.Show(validation.Message, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_categoryService.CategoryNameExists(Name.Trim()))
            {
                MessageBox.Show("Category name already exists.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _categoryService.AddCategory(new CategoryModel
            {
                Name = Name.Trim(),
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                IsActive = IsActive,
                CreatedAt = DateTime.Now
            });
            ClearForm();
            LoadCategories();
        }

        private void ExecuteUpdate()
        {
            if (SelectedCategory is null)
            {
                MessageBox.Show("Select a category first.", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var validation = Validate();
            if (!validation.IsValid)
            {
                MessageBox.Show(validation.Message, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_categoryService.CategoryNameExists(Name.Trim(), SelectedCategory.Id))
            {
                MessageBox.Show("Category name already exists.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var category = SelectedCategory;

            category.Name = Name.Trim();
            category.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            category.IsActive = IsActive;
            category.UpdatedAt = DateTime.Now;

            _categoryService.UpdateCategory(category);
            ClearForm();
            LoadCategories();
        }

        private void ExecuteDelete(object? parameter)
        {
            if (parameter is not int id || MessageBox.Show("Delete this category?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            _categoryService.DeleteCategory(id);
            ClearForm();
            LoadCategories();
        }

        private void ClearForm()
        {
            SelectedCategory = null;
            Name = string.Empty;
            Description = string.Empty;
            IsActive = true;
        }

        public void Dispose() => _categoryService.Dispose();
    }
}