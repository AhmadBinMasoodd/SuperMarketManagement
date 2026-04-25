using System.Windows;
using System.Windows.Controls;
using SuperMarketManagement.Models;

namespace SuperMarketManagement.Views.Admin
{
    /// <summary>
    /// Interaction logic for AdminDashboard.xaml
    /// </summary>
    public partial class AdminDashboard : Window
    {
        private readonly User _user;
        private ContentControl? MainHost => FindName("MainContentHost") as ContentControl;
        private Button? DashboardBtn => FindName("DashboardButton") as Button;
        private Button? EmployeesBtn => FindName("EmployeesButton") as Button;
        private Button? CategoryBtn => FindName("CategoryButton") as Button;
        private Button? ProductMenuBtn => FindName("ProductBtn") as Button;
        private Button? StockHistoryMenuBtn => FindName("StockHistoryBtn") as Button;

        public AdminDashboard(User user)
        {
            InitializeComponent();
            _user = user;

            if (EmployeesBtn is not null)
            {
                EmployeesBtn.Click -= EmployeesButton_Click;
                EmployeesBtn.Click += EmployeesButton_Click;
            }

            if (DashboardBtn is not null)
            {
                DashboardBtn.Click -= DashboardButton_Click;
                DashboardBtn.Click += DashboardButton_Click;
            }

            if (CategoryBtn is not null)
            {
                CategoryBtn.Click -= CategoryButton_Click;
                CategoryBtn.Click += CategoryButton_Click;
            }

            if (ProductMenuBtn is not null)
            {
                ProductMenuBtn.Click -= ProductButton_Click;
                ProductMenuBtn.Click += ProductButton_Click;
            }

            if (StockHistoryMenuBtn is not null)
            {
                StockHistoryMenuBtn.Click -= StockHistoryButton_Click;
                StockHistoryMenuBtn.Click += StockHistoryButton_Click;
            }

            LoadView(new ChartOverview(), DashboardBtn);
        }

        private void EmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            LoadView(new Employee(), EmployeesBtn);
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            LoadView(new ChartOverview(), DashboardBtn);
        }

        private void CategoryButton_Click(object sender, RoutedEventArgs e)
        {
            LoadView(new Category(), CategoryBtn);
        }

        private void ProductButton_Click(object sender, RoutedEventArgs e)
        {
            LoadView(new Product(_user), ProductMenuBtn);
        }

        private void StockHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            LoadView(new StockHistory(_user), StockHistoryMenuBtn);
        }

        private void LoadView(UserControl view, Button? activeButton)
        {
            if (MainHost is not null)
            {
                MainHost.Content = view;
            }

            ApplyMenuState(activeButton);
        }

        private void ApplyMenuState(Button? activeButton)
        {
            if (DashboardBtn is not null)
            {
                DashboardBtn.Style = (Style)FindResource("SideMenuButtonStyle");
            }

            if (EmployeesBtn is not null)
            {
                EmployeesBtn.Style = (Style)FindResource("SideMenuButtonStyle");
            }

            if (CategoryBtn is not null)
            {
                CategoryBtn.Style = (Style)FindResource("SideMenuButtonStyle");
            }

            if (ProductMenuBtn is not null)
            {
                ProductMenuBtn.Style = (Style)FindResource("SideMenuButtonStyle");
            }

            if (StockHistoryBtn is not null)
            {
                StockHistoryBtn.Style = (Style)FindResource("SideMenuButtonStyle");
            }

            if (activeButton is not null)
            {
                activeButton.Style = (Style)FindResource("SideMenuButtonActiveStyle");
            }
        }
    }
}
