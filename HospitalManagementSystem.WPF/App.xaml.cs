using HospitalManagementSystem.Infrastructure.Context;
using HospitalManagementSystem.Infrastructure.Identity;
using HospitalManagementSystem.Infrastructure.Identity.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Windows;

namespace HospitalManagementSystem.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider serviceProvider = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["HospitalDb"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'HospitalDb' was not found in App.config.");
            }


            ///Db Context
            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

            //Identity Setup

            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            serviceProvider = services.BuildServiceProvider();

            SeedInitialData();
            base.OnStartup(e);
        }

        private static void SeedInitialData()
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            RoleSeeder.SeedRoles(roleManager).GetAwaiter().GetResult();
            AdminSeeder.SeedAdmin(userManager, roleManager).GetAwaiter().GetResult();
        }
    }

}
