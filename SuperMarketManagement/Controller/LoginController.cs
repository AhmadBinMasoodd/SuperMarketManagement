using SuperMarketManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperMarketManagement.Controller
{
    public class LoginController
    {
        private readonly MarketDbContext _context = new();
        public LoginController()
        {

        }
        public User? Authenticate(string username,string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user is null)
            {
                return null;
            }

            return user;
        }
    }
}
