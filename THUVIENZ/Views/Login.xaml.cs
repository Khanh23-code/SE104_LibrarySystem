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
                // Láº¥y thÃ´ng tin tá»« UI
                string username = txtUserID.Text;
                string password = txtPassword.Password;

                // Gá» i BLL xá» lÃ½ Ä‘Äƒng nháº­p
                string? role = _authService.Login(username, password);

                if (role != null)
                {
                    // LÆ°u thÃ´ng tin vÃ o Session
                    UserSession.Username = username;
                    UserSession.Role = role;

                    // Ä Äƒng nháº­p thÃ nh cÃ´ng
                    MessageBox.Show(
                        $"Ä Äƒng nháº­p thÃ nh cÃ´ng! ChÃ o má»«ng báº¡n quay trá»Ÿ láº¡i vá»›i quyá» n: {role}", 
                        "ThÃ´ng bÃ¡o", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);

                    // Chuyá»ƒn sang trang Profile cá»§a Reader (hoáº·c Admin Dashboard tÆ°Æ¡ng á»©ng)
                    Profile profileWindow = new Profile();
                    profileWindow.Show();
                    this.Close();
                }
                else
                {
                    // Ä Äƒng nháº­p tháº¥t báº¡i
                    MessageBox.Show(
                        "TÃªn Ä‘Äƒng nháº­p hoáº·c máº­t kháº©u khÃ´ng chÃ­nh xÃ¡c. Vui lÃ²ng thá» lÃ¡i.", 
                        "Lá»—i Ä‘Äƒng nháº­p", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                // Xá» lÃ½ lá»—i há»‡ thá»‘ng
                MessageBox.Show(
                    $"Ä Ã£ xáº£y ra lá»—i há»‡ thá»‘ng: {ex.Message}", 
                    "Lá»—i káº¿t ná»‘i", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
            }
        }

        #region Logic PasswordBox
        // Khi ngÆ°á»i dÃ¹ng gÃµ vÃ o Ã´ áº©n (PasswordBox)
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Chá»‰ cáº­p nháº­t náº¿u giÃ¡ trá»‹ khÃ¡c nhau Ä‘á»ƒ trÃ¡nh vÃ²ng láº·p vÃ´ táº­n
            if (txtPasswordVisible.Text != txtPassword.Password)
            {
                txtPasswordVisible.Text = txtPassword.Password;
            }

            Placeholder.Visibility = txtPassword.Password.Length > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        // Khi ngÆ°á»i dÃ¹ng gÃµ vÃ o Ã´ hiá»‡n (TextBox)
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtPassword.Password != txtPasswordVisible.Text)
            {
                txtPassword.Password = txtPasswordVisible.Text;
                txtPasswordVisible.SelectionStart = txtPasswordVisible.Text.Length;
            }

            Placeholder.Visibility = txtPasswordVisible.Text.Length > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        // Logic khi báº¥m vÃ o con máº¯t
        private void BtnShowPassword_Click(object sender, RoutedEventArgs e)
        {
            if (txtPasswordVisible.Visibility == Visibility.Collapsed)
            {
                // --- CHáº¾ Äá»˜ HIá»†N ---
                txtPasswordVisible.Visibility = Visibility.Visible;
                txtPassword.Visibility = Visibility.Collapsed;
                btnShowPassword.Content = "ðŸ”’";

                txtPasswordVisible.Focus();

                // ÄÆ°a con trá» cá»§a TextBox vá» cuá»‘i
                txtPasswordVisible.CaretIndex = txtPasswordVisible.Text.Length;
            }
            else
            {
                // --- CHáº¾ Äá»˜ áº¨N ---
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                txtPassword.Visibility = Visibility.Visible;
                btnShowPassword.Content = "ðŸ‘";

                txtPassword.Focus();

                // FIX BUG: DÃ¹ng Reflection Ä‘á»ƒ gá»i hÃ m Select ná»™i bá»™ cá»§a PasswordBox
                var selectMethod = typeof(PasswordBox).GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic);
                if (selectMethod != null)
                {
                    // Tham sá»‘ 1: Vá»‹ trÃ­ báº¯t Ä‘áº§u (Ä‘áº·t á»Ÿ cuá»‘i chuá»—i)
                    // Tham sá»‘ 2: Äá»™ dÃ i vÃ¹ng chá»n (0 = chá»‰ Ä‘áº·t con trá», khÃ´ng bÃ´i Ä‘en)
                    selectMethod.Invoke(txtPassword, new object[] { txtPassword.Password.Length, 0 });
                }
            }
        }

        // CÃ²n bug: Khi báº¥m vÃ o con máº¯t Ä‘á»ƒ áº©n máº­t kháº©u, con trá» sáº½ bá»‹ nháº£y vá» Ä‘áº§u dÃ²ng.
        #endregion
    }
}
