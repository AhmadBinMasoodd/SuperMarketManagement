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
        public string Authenticate(string username,string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user is null)
            {
                return string.Empty;
            }

            string role = user.Role;

            return role;
        }
    }
}
