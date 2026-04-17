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
        private string _idError = string.Empty;
        public string IdError
        {
            get => _idError;
            set { _idError = value; OnPropertyChanged(); }
        }

        private string _passwordError = string.Empty;
        public string PasswordError 
        {             
            get => _passwordError;
            set { _passwordError = value; OnPropertyChanged(); }
        }

        private string _id = string.Empty;
        public string Id
        {
            get => _id;
            set
            {
                _id = value;

                if (string.IsNullOrEmpty(_id))
                    IdError = "Mã số không được để trống.";
                else if (!InputValidator.IsValidId(_id))
                    IdError = "Mã số không chứa khoảng trắng.";
                else
                    IdError = string.Empty;

                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested(); // Cập nhật trạng thái của các command liên quan nếu có
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;

                if (string.IsNullOrEmpty(_password))
                    PasswordError = "Mật khẩu không được để trống.";
                else if (!InputValidator.IsValidPassword(_password))
                    PasswordError = "Mật khẩu phải từ 6 ký tự và không có khoảng trắng.";
                else
                    PasswordError = string.Empty;

                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand LoginCommand { get; }

        private readonly AuthService _authService;

        public LoginViewModel()
        {
            _authService = new AuthService();
            LoginCommand = new RelayCommand(ExecuteLogin, CanExcuteLogin);
        }

        private bool CanExcuteLogin(object parameter)
        {
            if (String.IsNullOrEmpty(Id) || String.IsNullOrEmpty(Password))
                return false;
            if (!InputValidator.IsValidId(Id) || !InputValidator.IsValidPassword(Password))
                return false;

            return true;
        }

        private void ExecuteLogin(object parameter)
        {
            try
            {
                // Gọi BLL để xử lý đăng nhập
                string? role = _authService.Login(Id, Password);

                if (role != null)
                {
                    // Thông báo thành công
                    MessageBox.Show(
                        $"{role} đăng nhập thành công! Chào mừng bạn quay trở lại.", 
                        "Thành công", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
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