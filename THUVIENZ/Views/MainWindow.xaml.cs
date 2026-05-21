using System.Windows;
using System.Windows.Controls;
using THUVIENZ.Views;
using THUVIENZ.Views.Components;
using THUVIENZ.Core;

namespace THUVIENZ
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ApplyRouting();
        }

        private void ApplyRouting()
        {
            string role = UserSession.Role ?? "";
            if (string.IsNullOrWhiteSpace(role))
            {
                // Chưa đăng nhập: chuyển về Login mà không hiện thông báo lỗi phân quyền
                new Login().Show();
                this.Close();
                return;
            }

            string checkRole = role.ToUpper();

            // Nếu Backend trả về Role là "ADMIN" hoặc "QUANLY" (bạn tự chỉnh cho khớp chữ DB nhé)
            if (checkRole == "ADMIN")
            {
                var adminNav = new AdminNavigationBar();
                adminNav.OnNavigate += (page, name) => HandleNavigation(adminNav, page, name);
                NavContainer.Content = adminNav;
                HandleNavigation(adminNav, new AdminBooks(), "Books");
            }
            // Nếu Backend trả về Role là "READER" hoặc "DOCGIA"
            else if (checkRole == "READER")
            {
                var readerNav = new NavigationBar();
                readerNav.OnNavigate += (page, name) => HandleNavigation(readerNav, page, name);
                NavContainer.Content = readerNav;
                HandleNavigation(readerNav, new Profile(), "Profile");
            }
            else
            {
                // Role không hợp lệ thì đá văng ra ngoài Login
                MessageBox.Show("Tài khoản của bạn không được cấp quyền truy cập hợp lệ!", "Lỗi phân quyền", MessageBoxButton.OK, MessageBoxImage.Error);
                new Login().Show();
                this.Close();
            }
        }

        // Hàm xử lý điều hướng dùng chung cho cả Admin và Reader
        private void HandleNavigation(UserControl navBar, UserControl newPage, string pageName)
        {
            // Hiển thị nội dung trang mới vào cột phải
            MainContent.Content = newPage;

            // Đồng bộ trạng thái Highlight trên thanh Nav đang hiển thị
            if (navBar is AdminNavigationBar adminNav)
                adminNav.ActivePage = pageName;
            else if (navBar is NavigationBar readerNav)
                readerNav.ActivePage = pageName;

            if (newPage is Notifications notificationsPage)
            {
                notificationsPage.OnNotificationsViewed += () =>
                {
                    // Nếu thanh Nav hiện tại là của Reader, ra lệnh xóa chấm đỏ
                    if (navBar is NavigationBar readerMenu)
                    {
                        readerMenu.ClearRedDot();
                    }
                };
            }
        }
    }
}