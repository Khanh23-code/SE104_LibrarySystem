using System;
using System.Windows;
using System.Windows.Input;

namespace THUVIENZ.Views
{
    public partial class ForgotPassword : Window
    {
        public ForgotPassword()
        {
            InitializeComponent();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUserID.Text;
            string newPass = txtNewPassword.Password;
            string confirmPass = txtConfirmPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(newPass))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPass != confirmPass)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // TODO: Gọi Service xử lý đổi mật khẩu tại đây
            MessageBox.Show("Yêu cầu của bạn đã được gửi đi. Vui lòng chờ phản hồi từ hệ thống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

            new Login().Show();
            this.Close();
        }

        private void BtnBackToLogin_Click(object sender, MouseButtonEventArgs e)
        {
            new Login().Show();
            this.Close();
        }
    }
}