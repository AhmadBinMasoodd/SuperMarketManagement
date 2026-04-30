using System;
using System.Windows;
using System.Windows.Controls;
using SuperMarketManagement.Models;
using System.Linq;

namespace SuperMarketManagement.Views.Admin
{
    public partial class Profile : UserControl
    {
        private readonly MarketDbContext _context = new();
        private readonly User _currentUser;

        public Profile(User user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadProfile();
        }

        private void LoadProfile()
        {
            var user = _context.Users.Find(_currentUser.Id) ?? _currentUser;
            if (NameInput != null) NameInput.Text = user.Name;
            if (AgeInput != null) AgeInput.Text = user.Age.ToString();
            if (AddressInput != null) AddressInput.Text = user.Address;
            if (UsernameInput != null) UsernameInput.Text = user.Username;
            if (PasswordInput != null) PasswordInput.Text = user.Password;

            if (GenderInput != null)
            {
                foreach (ComboBoxItem item in GenderInput.Items)
                {
                    if (string.Equals(item.Content?.ToString(), user.Gender, StringComparison.OrdinalIgnoreCase))
                    {
                        GenderInput.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var user = _context.Users.Find(_currentUser.Id);
            if (user == null) return;

            user.Name = NameInput?.Text.Trim() ?? string.Empty;
            if (int.TryParse(AgeInput?.Text, out int age)) user.Age = age;
            user.Gender = (GenderInput?.SelectedItem as ComboBoxItem)?.Content?.ToString();
            user.Address = AddressInput?.Text.Trim();
            user.Username = UsernameInput?.Text.Trim() ?? string.Empty;
            user.Password = PasswordInput?.Text.Trim() ?? string.Empty;

            _context.SaveChanges();
            MessageBox.Show("Profile updated successfully.");
        }
    }
}

