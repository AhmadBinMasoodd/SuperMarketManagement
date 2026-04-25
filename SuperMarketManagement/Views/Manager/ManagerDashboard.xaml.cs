using SuperMarketManagement.Models;
using SuperMarketManagement.Views.Admin;
using System.Windows;
using System.Windows.Controls;

namespace SuperMarketManagement.Views.Manager
{
    /// <summary>
    /// Interaction logic for ManagerDashboard.xaml
    /// </summary>
    public partial class ManagerDashboard : Window
    {
        private readonly User _user;
        private ContentControl? MainHost => FindName("MainContentHost") as ContentControl;
        private Button? DashboardBtn => FindName("DashboardButton") as Button;
        private Button? CategoryBtn => FindName("CategoryButton") as Button;
        private Button? ProductMenuBtn => FindName("ProductBtn") as Button;
        private Button? StockHistoryMenuBtn => FindName("StockHistoryBtn") as Button;

        public ManagerDashboard(User user)
        {
            InitializeComponent();
            _user = user;

            if (user != null)
            {
                var nameText = FindName("ManagerNameText") as TextBlock;
                var roleText = FindName("ManagerRoleText") as TextBlock;
                if (nameText != null) nameText.Text = user.Name;
                if (roleText != null) roleText.Text = user.Role;
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

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            LoadView(new ChartOverview(), DashboardBtn);
        }

        private void CategoryButton_Click(object sender, RoutedEventArgs e)
        {
            LoadView(new Views.Admin.Category(), CategoryBtn);
        }

        private void ProductButton_Click(object sender, RoutedEventArgs e)
        {
            LoadView(new Views.Admin.Product(_user), ProductMenuBtn);
        }

        private void StockHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            LoadView(new Views.Admin.StockHistory(_user), StockHistoryMenuBtn);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
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
