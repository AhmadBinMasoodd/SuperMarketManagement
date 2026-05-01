using System;
using System.Linq;
using SuperMarketManagement.Models;

namespace SuperMarketManagement.Services
{
    public sealed class AuthService : IDisposable
    {
        private readonly MarketDbContext _context = new();

        public User? Authenticate(string username, string password)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
        }

        public void Dispose() => _context.Dispose();
    }
}
