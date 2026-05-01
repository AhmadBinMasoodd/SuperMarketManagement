using System;
using System.Collections.Generic;
using System.Linq;
using SuperMarketManagement.Models;

namespace SuperMarketManagement.Services
{
    public sealed class EmployeeService : IDisposable
    {
        private readonly MarketDbContext _context = new();

        public IReadOnlyList<User> GetEmployees()
        {
            return _context.Users
                .Where(u => u.Role == "Cashier" || u.Role == "Manager")
                .OrderBy(u => u.Id)
                .ToList();
        }

        public void AddEmployee(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void UpdateEmployee(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void DeleteEmployee(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        public User? GetById(int id) => _context.Users.Find(id);

        public void Dispose() => _context.Dispose();
    }
}
