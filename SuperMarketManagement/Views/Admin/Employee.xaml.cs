using System;
using System.Windows;
using System.Windows.Controls;
using SuperMarketManagement.Controller;
using SuperMarketManagement.Models;

namespace SuperMarketManagement.Views.Admin
{
    /// <summary>
    /// Interaction logic for Employee.xaml
    /// </summary>
    public partial class Employee : UserControl
    {
        private readonly EmployeeController _controller = new();
        private User? _selectedUser;
        private DataGrid? EmployeeGrid => FindName("EmployeeDataGrid") as DataGrid;
        private TextBox? NameInput => FindName("NameTextBox") as TextBox;
        private TextBox? AgeInput => FindName("AgeTextBox") as TextBox;
        private ComboBox? GenderInput => FindName("GenderComboBox") as ComboBox;
        private ComboBox? RoleInput => FindName("RoleComboBox") as ComboBox;
        private TextBox? AddressInput => FindName("AddressTextBox") as TextBox;
        private TextBox? UsernameInput => FindName("UsernameTextBox") as TextBox;
        private TextBox? PasswordInput => FindName("PasswordTextBox") as TextBox;
        private TextBox? SalaryInput => FindName("SalaryTextBox") as TextBox;
        private Button? AddBtn => FindName("AddButton") as Button;
        private Button? UpdateBtn => FindName("UpdateButton") as Button;

        public Employee()
        {
            InitializeComponent();
            WireEvents();
            LoadEmployees();
        }

        ~Employee()
        {
            _controller.Dispose();
        }

        private void WireEvents()
        {
            if (EmployeeGrid is not null)
            {
                EmployeeGrid.SelectionChanged -= EmployeeDataGrid_SelectionChanged;
                EmployeeGrid.SelectionChanged += EmployeeDataGrid_SelectionChanged;
            }

            if (AddBtn is not null)
            {
                AddBtn.Click -= AddButton_Click;
                AddBtn.Click += AddButton_Click;
            }

            if (UpdateBtn is not null)
            {
                UpdateBtn.Click -= UpdateButton_Click;
                UpdateBtn.Click += UpdateButton_Click;
            }
        }

        private void LoadEmployees()
        {
            if (EmployeeGrid is null)
            {
                return;
            }

            EmployeeGrid.ItemsSource = _controller.GetEmployees();
        }

        private void EmployeeDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmployeeGrid?.SelectedItem is not User user)
            {
                return;
            }

            FillForm(user);
        }

        private void FillForm(User user)
        {
            if (NameInput is null || AgeInput is null || AddressInput is null || UsernameInput is null || PasswordInput is null || SalaryInput is null || GenderInput is null || RoleInput is null)
            {
                return;
            }

            _selectedUser = user;
            NameInput.Text = user.Name;
            AgeInput.Text = user.Age.ToString();
            AddressInput.Text = user.Address;
            UsernameInput.Text = user.Username;
            PasswordInput.Text = user.Password;
            SalaryInput.Text = user.Salary?.ToString() ?? string.Empty;
            SetComboSelection(GenderInput, user.Gender);
            SetComboSelection(RoleInput, user.Role);
        }

        private static void SetComboSelection(ComboBox comboBox, string value)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (string.Equals(item.Content?.ToString(), value, StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }

            comboBox.SelectedIndex = -1;
        }

        private string? GetSelectedComboValue(ComboBox comboBox)
        {
            return (comboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        }

        private EmployeeInput? BuildInput()
        {
            if (NameInput is null || AgeInput is null || AddressInput is null || UsernameInput is null || PasswordInput is null || SalaryInput is null || GenderInput is null || RoleInput is null)
            {
                return null;
            }

            return new EmployeeInput
            {
                Name = NameInput.Text.Trim(),
                AgeText = AgeInput.Text.Trim(),
                Gender = GetSelectedComboValue(GenderInput) ?? string.Empty,
                Address = AddressInput.Text.Trim(),
                Role = GetSelectedComboValue(RoleInput) ?? string.Empty,
                Username = UsernameInput.Text.Trim(),
                Password = PasswordInput.Text.Trim(),
                SalaryText = SalaryInput.Text.Trim()
            };
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
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

            _controller.AddEmployee(input);
            LoadEmployees();
            ClearForm();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser is null)
            {
                MessageBox.Show("Select an employee first.", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
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

            var isUpdated = _controller.UpdateEmployee(_selectedUser.Id, input);
            if (!isUpdated)
            {
                MessageBox.Show("Employee not found.", "Update", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LoadEmployees();
            ClearForm();
        }

        private void DeleteRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: int id })
            {
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this employee?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            _controller.DeleteEmployee(id);
            LoadEmployees();
            ClearForm();
        }

        private void ClearForm()
        {
            _selectedUser = null;
            NameInput?.Clear();
            AgeInput?.Clear();
            AddressInput?.Clear();
            UsernameInput?.Clear();
            PasswordInput?.Clear();
            SalaryInput?.Clear();
            if (GenderInput is not null) GenderInput.SelectedIndex = -1;
            if (RoleInput is not null) RoleInput.SelectedIndex = -1;
            if (EmployeeGrid is not null) EmployeeGrid.SelectedItem = null;
        }
    }
}
