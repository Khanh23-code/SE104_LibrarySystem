using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using THUVIENZ.Views;
using THUVIENZ.Views.Components;

namespace THUVIENZ
{
    public partial class NavigationBar : UserControl
    {
        public static readonly DependencyProperty ActivePageProperty =
            DependencyProperty.Register("ActivePage", typeof(string), typeof(NavigationBar), new PropertyMetadata(string.Empty));

        public string ActivePage
        {
            get { return (string)GetValue(ActivePageProperty); }
            set { SetValue(ActivePageProperty, value); }
        }

        public event Action<UserControl, string>? OnNavigate;

        public NavigationBar()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateRedDotVisibility();
        }

        public async void UpdateRedDotVisibility()
        {
            var username = Core.UserSession.UserID;
            if (string.IsNullOrEmpty(username))
            {
                RedDotBadge.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                // Thực hiện quét hạn trả sách sắp hết hạn cho độc giả
                await BLL.NotificationService.SweepLoomingDueDatesAsync(username);

                using var context = new DAL.LmsDbContext();
                var hasUnread = await context.ThongBaos.AnyAsync(t => t.TenDangNhap == username && !t.DaDoc);
                RedDotBadge.Visibility = hasUnread ? Visibility.Visible : Visibility.Collapsed;
            }
            catch
            {
                RedDotBadge.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            // "Báo cáo" ra bên ngoài là tôi muốn chuyển sang trang Profile
            OnNavigate?.Invoke(new Profile(), "Profile");
        }

        private void BtnFavorite_Click(object sender, RoutedEventArgs e)
        {
            OnNavigate?.Invoke(new Favorite(), "Favorite");
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            OnNavigate?.Invoke(new Search(), "Search");
        }

        private void BtnBorrowing_Click(object sender, RoutedEventArgs e)
        {
            OnNavigate?.Invoke(new Borrowing(), "Borrowing");
        }

        private void BtnNotifications_Click(object sender, RoutedEventArgs e)
        {
            OnNavigate?.Invoke(new Notifications(), "Notifications");
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Riêng Logout thì vẫn đóng MainWindow và mở Login
            new Login().Show();
            Window.GetWindow(this)?.Close();
        }

        public void ClearRedDot()
        {
            RedDotBadge.Visibility = Visibility.Collapsed;
        }
    }
}
