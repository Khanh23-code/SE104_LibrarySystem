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

        #region Logic PasswordBox
        // Khi người dùng gõ vào ô ẩn (PasswordBox)
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Chỉ cập nhật nếu giá trị khác nhau để tránh vòng lặp vô tận
            if (txtPasswordVisible.Text != txtPassword.Password)
            {
                txtPasswordVisible.Text = txtPassword.Password;
            }

            Placeholder.Visibility = txtPassword.Password.Length > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        // Khi người dùng gõ vào ô hiện (TextBox)
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtPassword.Password != txtPasswordVisible.Text)
            {
                txtPassword.Password = txtPasswordVisible.Text;
                txtPasswordVisible.SelectionStart = txtPasswordVisible.Text.Length;
            }

            Placeholder.Visibility = txtPasswordVisible.Text.Length > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        // Logic khi bấm vào con mắt
        private void BtnShowPassword_Click(object sender, RoutedEventArgs e)
        {
            if (txtPasswordVisible.Visibility == Visibility.Collapsed)
            {
                // --- CHẾ ĐỘ HIỆN ---
                txtPasswordVisible.Visibility = Visibility.Visible;
                txtPassword.Visibility = Visibility.Collapsed;
                btnShowPassword.Content = "🔒";

                txtPasswordVisible.Focus();

                // Đưa con trỏ của TextBox về cuối
                txtPasswordVisible.CaretIndex = txtPasswordVisible.Text.Length;
            }
            else
            {
                // --- CHẾ ĐỘ ẨN ---
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                txtPassword.Visibility = Visibility.Visible;
                btnShowPassword.Content = "👁";

                txtPassword.Focus();

                // FIX BUG: Dùng Reflection để gọi hàm Select nội bộ của PasswordBox
                var selectMethod = typeof(PasswordBox).GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic);
                if (selectMethod != null)
                {
                    // Tham số 1: Vị trí bắt đầu (đặt ở cuối chuỗi)
                    // Tham số 2: Độ dài vùng chọn (0 = chỉ đặt con trỏ, không bôi đen)
                    selectMethod.Invoke(txtPassword, new object[] { txtPassword.Password.Length, 0 });
                }
            }
        }
        #endregion
    }
}