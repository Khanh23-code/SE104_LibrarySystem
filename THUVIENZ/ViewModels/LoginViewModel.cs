using System;
using System.Windows;
using System.Windows.Input;
using THUVIENZ.BLL;
using THUVIENZ.Core;

namespace THUVIENZ.ViewModels
{
    /// <summary>
    /// ViewModel cho màn hình Đăng nhập.
    /// Tuân thủ quy tắc 3-Tier: ViewModel chỉ xử lý logic UI và gọi xuống BLL.
    /// </summary>
    public class LoginViewModel : ObservableObject
    {
        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoginCommand { get; }

        private readonly AuthService _authService;

        public LoginViewModel()
        {
            _authService = new AuthService();
            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        private void ExecuteLogin(object parameter)
        {
            try
            {
                // Gọi BLL để xử lý đăng nhập
                string? role = _authService.Login(Username, Password);

                if (role != null)
                {
                    // Thông báo thành công (Spirit: WOW aesthetics - thông báo rõ ràng)
                    MessageBox.Show(
                        $"Chào mừng quay trở lại! Đăng nhập thành công với quyền: {role}", 
                        "Thành công", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);

                    // TODO: Thực hiện điều hướng màn hình tùy theo quyền (Admin -> Dashboard, Reader -> Search)
                }
                else
                {
                    // Thông báo lỗi
                    MessageBox.Show(
                        "Tên đăng nhập hoặc mật khẩu không chính xác. Vui lòng thử lại.", 
                        "Đăng nhập thất bại", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi hệ thống/kết nối
                MessageBox.Show(
                    $"Đã xảy ra lỗi hệ thống: {ex.Message}", 
                    "Lỗi kết nối", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
            }
        }
    }
}
