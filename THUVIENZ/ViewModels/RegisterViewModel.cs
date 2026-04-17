using System.Windows;
using System.Windows.Input;
using THUVIENZ.BLL;
using THUVIENZ.Core;

namespace THUVIENZ.ViewModels
{
    public class RegisterViewModel : ObservableObject
    {
        private string _fullNameError = string.Empty;
        public string FullNameError
        {
            get => _fullNameError;
            set { _fullNameError = value; OnPropertyChanged(); }
        }

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

        private string _fullName = string.Empty;
        public string FullName
        {
            get => _fullName;
            set 
            { 
                _fullName = value;

                if (string.IsNullOrWhiteSpace(_fullName))
                    FullNameError = "Họ tên không được để trống.";
                else if (!InputValidator.IsValidName(_fullName))
                    FullNameError = "Họ tên chỉ được chứa chữ cái.";
                else
                    FullNameError = string.Empty; // Biến mất khi hợp lệ

                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested(); // Cập nhật trạng thái của các command liên quan nếu có
            }
        }

        private string _id = string.Empty;
        public string Id
        {
            get => _id;
            set 
            { 
                _id = value;

                if (string.IsNullOrWhiteSpace(_id))
                    IdError = "Mã số không được để trống.";
                else if (!InputValidator.IsValidId(_id))
                    IdError = "Mã số không chứa khoảng trắng.";
                else
                    IdError = string.Empty;
                OnPropertyChanged(); 
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                if (!InputValidator.IsValidPassword(_password))
                    PasswordError = "Mật khẩu phải từ 6 ký tự và không có khoảng trắng.";
                else
                    PasswordError = string.Empty;

                OnPropertyChanged();
                CheckPasswordMatch();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private string _confirmPassword = string.Empty;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                if (!InputValidator.IsValidPassword(_confirmPassword))
                    PasswordError = "Mật khẩu phải từ 6 ký tự và không có khoảng trắng.";
                else
                    PasswordError = string.Empty;

                OnPropertyChanged();
                CheckPasswordMatch();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void CheckPasswordMatch()
        {
            if (!InputValidator.IsPasswordMatch(Password, ConfirmPassword))
                PasswordError = "Mật khẩu xác nhận không khớp.";
            else if (InputValidator.IsValidPassword(Password))
                PasswordError = string.Empty;
        }

        public ICommand RegisterCommand { get; }
        // private readonly AuthService _authService; // Uncomment khi bạn nối BLL

        public RegisterViewModel()
        {
            // Khởi tạo các command nếu sau này team muốn chuyển hẳn sang MVVM thuần
            RegisterCommand = new RelayCommand(ExecuteRegister, CanExecuteRegister);
        }

        /// <summary>
        /// Nút Đăng ký chỉ sáng lên (Enable) khi hàm này trả về TRUE
        /// </summary>
        private bool CanExecuteRegister(object? parameter)
        {
            if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword))
                return false;

            if (!InputValidator.IsValidName(FullName) || !InputValidator.IsValidId(Id) || !InputValidator.IsValidPassword(Password)) 
                return false;

            if (!InputValidator.IsPasswordMatch(Password, ConfirmPassword)) return false;

            return true;
        }

        private void ExecuteRegister(object? parameter)
        {
            // Code xử lý đăng ký thực tế (gọi BLL) sẽ nằm ở đây
            // Vì CanExecuteRegister đã bắt hết lỗi, xuống tới đây dữ liệu chắc chắn đã sạch.

            MessageBox.Show("Dữ liệu hợp lệ! Sẵn sàng đẩy xuống Database.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}