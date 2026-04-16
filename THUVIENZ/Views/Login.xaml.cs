using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Reflection;
using THUVIENZ.BLL;
using THUVIENZ.Core;
using System;

namespace THUVIENZ.Views
{
    public partial class Login : Window
    {
        private readonly AuthService _authService;
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        private const uint EM_SETSEL = 0x00B1;

        public Login()
        {
            InitializeComponent();
            _authService = new AuthService();
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Lấy thông tin từ UI
                string username = txtUserID.Text;
                string password = txtPassword.Password;

                // Gọi BLL xử lý đăng nhập
                string? role = _authService.Login(username, password);

                if (role != null)
                {
                    // Lưu thông tin vào Session
                    UserSession.Username = username;
                    UserSession.Role = role;

                    // Đăng nhập thành công
                    MessageBox.Show(
                        $"{role} đăng nhập thành công! Chào mừng bạn quay trở lại.", 
                        "Thông báo", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);

                    // Chuyển sang trang Profile của Reader (hoặc Admin Dashboard tương ứng)
                    Profile profileWindow = new Profile();
                    profileWindow.Show();
                    this.Close();
                }
                else
                {
                    // Đăng nhập thất bại
                    MessageBox.Show(
                        "Tên đăng nhập hoặc mật khẩu không chính xác. Vui lòng thử lại.", 
                        "Lỗi đăng nhập", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi hệ thống
                MessageBox.Show(
                    $"Đã xảy ra lỗi hệ thống: {ex.Message}", 
                    "Lỗi kết nối", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
            }
        }

        private void LnkRegister_Click(object sender, RoutedEventArgs e)
        {
            Register registerWindow = new Register();
            registerWindow.Show();
            this.Close();
        }

        private void LnkForgot_Click(object sender, RoutedEventArgs e)
        {
            ForgotPassword forgotWindow = new ForgotPassword();
            forgotWindow.Show();
            this.Close();
        }
    }
}