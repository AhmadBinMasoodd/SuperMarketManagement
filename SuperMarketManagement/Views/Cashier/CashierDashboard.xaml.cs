using System.Windows;
using System.Windows.Controls;
using SuperMarketManagement.Models;

namespace SuperMarketManagement.Views.Cashier
{
    public partial class CashierDashboard : Window
    {
        private readonly User _user;
        private ContentControl? MainHost => FindName("MainContentHost") as ContentControl;
        private Button? POSMenuBtn => FindName("POSBtn") as Button;
        private Button? ReturnRefundMenuBtn => FindName("ReturnRefundBtn") as Button;
        private Button? DailySummaryMenuBtn => FindName("DailySummaryBtn") as Button;
        private Button? ProfileMenuBtn => FindName("ProfileBtn") as Button;

        public CashierDashboard(User user)
        {
            InitializeComponent();
            _user = user;

            if (user != null)
            {
                var nameText = FindName("CashierNameText") as TextBlock;
                if (nameText != null) nameText.Text = user.Name;

                var roleText = FindName("CashierRoleText") as TextBlock;
                if (roleText != null) roleText.Text = user.Role;
            }

            if (POSMenuBtn is not null)
            {
                POSMenuBtn.Click -= POSButton_Click;
                POSMenuBtn.Click += POSButton_Click;
            }

            if (ReturnRefundMenuBtn is not null)
            {
                ReturnRefundMenuBtn.Click -= ReturnRefundButton_Click;
                ReturnRefundMenuBtn.Click += ReturnRefundButton_Click;
            }

            if (DailySummaryMenuBtn is not null)
            {
                DailySummaryMenuBtn.Click -= DailySummaryButton_Click;
                DailySummaryMenuBtn.Click += DailySummaryButton_Click;
            }

            if (ProfileMenuBtn is not null)
            {
                ProfileMenuBtn.Click -= ProfileButton_Click;
                ProfileMenuBtn.Click += ProfileButton_Click;
            }

            LoadView(new POSView(_user), POSMenuBtn);
        }

        private void POSButton_Click(object sender, RoutedEventArgs e)
        {
            LoadView(new POSView(_user), POSMenuBtn);
        }

        private void ReturnRefundButton_Click(object sender, RoutedEventArgs e)
        {
            LoadView(new ReturnRefundView(), ReturnRefundMenuBtn);
        }

        private void DailySummaryButton_Click(object sender, RoutedEventArgs e)
        {
            LoadView(new DailySummaryView(), DailySummaryMenuBtn);
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            LoadView(new Views.Admin.Profile(_user), ProfileMenuBtn);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            var login = new MainWindow();
            login.Show();
            Close();
        }

        private void LoadView(UserControl view, Button? activeButton)
        {
            if (MainHost is not null)
            {
                MainHost.Content = view;
            }

            if (POSMenuBtn is not null)
            {
                POSMenuBtn.Style = (Style)FindResource("SideMenuButtonStyle");
            }

            if (ReturnRefundMenuBtn is not null)
            {
                ReturnRefundMenuBtn.Style = (Style)FindResource("SideMenuButtonStyle");
            }

            if (DailySummaryMenuBtn is not null)
            {
                DailySummaryMenuBtn.Style = (Style)FindResource("SideMenuButtonStyle");
            }

            if (ProfileMenuBtn is not null)
            {
                ProfileMenuBtn.Style = (Style)FindResource("SideMenuButtonStyle");
            }

            if (activeButton is not null)
            {
                activeButton.Style = (Style)FindResource("SideMenuButtonActiveStyle");
            }
        }
    }
}
