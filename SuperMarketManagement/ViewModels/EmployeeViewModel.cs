using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SuperMarketManagement.Models;
using SuperMarketManagement.ViewModels.Base;

namespace SuperMarketManagement.ViewModels
{
    public class EmployeeViewModel : ViewModelBase, IDisposable
    {
        private readonly MarketDbContext _context = new();

        private ObservableCollection<User> _employees = new();
        public ObservableCollection<User> Employees
        {
            get => _employees;
            set => SetProperty(ref _employees, value);
        }

        private User? _selectedEmployee;
        public User? SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                if (SetProperty(ref _selectedEmployee, value) && value != null)
                {
                    Name = value.Name;
                    Age = value.Age.ToString();
                    Gender = value.Gender;
                    Address = value.Address;
                    Role = value.Role;
                    Username = value.Username;
                    Password = value.Password;
                    Salary = value.Salary?.ToString() ?? string.Empty;
                }
            }
        }

        private string _name = string.Empty;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private string _age = string.Empty;
        public string Age { get => _age; set => SetProperty(ref _age, value); }

        private string _gender = string.Empty;
        public string Gender { get => _gender; set => SetProperty(ref _gender, value); }

        private string _address = string.Empty;
        public string Address { get => _address; set => SetProperty(ref _address, value); }

        private string _role = string.Empty;
        public string Role { get => _role; set => SetProperty(ref _role, value); }

        private string _username = string.Empty;
        public string Username { get => _username; set => SetProperty(ref _username, value); }

        private string _password = string.Empty;
        public string Password { get => _password; set => SetProperty(ref _password, value); }

        private string _salary = string.Empty;
        public string Salary { get => _salary; set => SetProperty(ref _salary, value); }

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }

        public EmployeeViewModel()
        {
            AddCommand = new RelayCommand(_ => ExecuteAdd());
            UpdateCommand = new RelayCommand(_ => ExecuteUpdate());
            DeleteCommand = new RelayCommand(_ => ExecuteDelete());
            ClearCommand = new RelayCommand(_ => ClearForm());
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            var users = _context.Users
                .Where(u => u.Role == "Cashier" || u.Role == "Manager")
                .OrderBy(u => u.Id)
                .ToList();
            Employees = new ObservableCollection<User>(users);
        }

        private void ExecuteAdd()
        {
            if (!Validate()) return;

            var user = new User
            {
                Name = Name,
                Age = int.Parse(Age),
                Gender = Gender,
                Address = Address,
                Role = Role,
                Username = Username,
                Password = Password,
                Salary = string.IsNullOrWhiteSpace(Salary) ? null : decimal.Parse(Salary)
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            LoadEmployees();
            ClearForm();
        }

        private void ExecuteUpdate()
        {
            if (SelectedEmployee == null || !Validate()) return;

            var user = _context.Users.Find(SelectedEmployee.Id);
            if (user != null)
            {
                user.Name = Name;
                user.Age = int.Parse(Age);
                user.Gender = Gender;
                user.Address = Address;
                user.Role = Role;
                user.Username = Username;
                user.Password = Password;
                user.Salary = string.IsNullOrWhiteSpace(Salary) ? null : decimal.Parse(Salary);

                _context.SaveChanges();
                LoadEmployees();
                ClearForm();
            }
        }

        private void ExecuteDelete()
        {
            if (SelectedEmployee == null) return;

            var result = MessageBox.Show("Are you sure you want to delete this employee?", "Confirm", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                var user = _context.Users.Find(SelectedEmployee.Id);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    _context.SaveChanges();
                    LoadEmployees();
                    ClearForm();
                }
            }
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Age) || 
                string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Please fill required fields.");
                return false;
            }
            return true;
        }

        private void ClearForm()
        {
            Name = Age = Gender = Address = Role = Username = Password = Salary = string.Empty;
            SelectedEmployee = null;
        }

        public void Dispose() => _context.Dispose();
    }
}
