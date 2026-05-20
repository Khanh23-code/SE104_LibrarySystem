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
        public event Action? OnLoginSuccess;

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
                    PasswordError = "Mật khẩu không có khoảng trắng.";
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

        private async void ExecuteLogin(object parameter)
        {
            try
            {
                // Gọi BLL để xử lý đăng nhập
                string? result = await _authService.LoginAsync(Id, Password);

                if (result == "PENDING_OR_LOCKED")
                {
                    MessageBox.Show("Tài khoản đang chờ duyệt hoặc bị khóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (result != null)
                {
                    // Lưu thông tin vào Session
                    UserSession.UserID = Id;
                    UserSession.Role = result;

                    // Thông báo thành công
                    MessageBox.Show(
                        $"{result} đăng nhập thành công! Chào mừng bạn quay trở lại.", 
                        "Thành công", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);

                    OnLoginSuccess?.Invoke();
                    UserSession.UserLoggedIn?.Invoke();
                }
                else
                {
                    MessageBox.Show("Tên đăng nhập hoặc mật khẩu không chính xác.", "Thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hệ thống: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}