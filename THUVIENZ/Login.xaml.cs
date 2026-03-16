using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace THUVIENZ
{
    public partial class Login : Window
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        private const uint EM_SETSEL = 0x00B1;

        public Login()
        {
            InitializeComponent();
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            // Chuyển sang trang Profile của Reader khi đăng nhập thành công
            Profile profileWindow = new Profile();
            profileWindow.Show();
            this.Close();
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

        // Còn bug: Khi bấm vào con mắt để ẩn mật khẩu, con trỏ sẽ bị nhảy về đầu dòng.
        #endregion
    }
}