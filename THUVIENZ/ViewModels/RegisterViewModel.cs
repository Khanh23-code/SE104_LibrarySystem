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

        private string _gender = "Khác";
        public string Gender
        {
            get => _gender;
            set { _gender = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); }
        }

        // Email/Phone/Address removed per spec

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
        private readonly AuthService _authService;

        public RegisterViewModel()
        {
            // Khởi tạo các command nếu sau này team muốn chuyển hẳn sang MVVM thuần
            _authService = new AuthService();
            // Use async command to avoid blocking UI thread
            RegisterCommand = new THUVIENZ.Commands.AsyncRelayCommand(ExecuteRegisterAsync, () => CanExecuteRegister(null));
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

        private async System.Threading.Tasks.Task ExecuteRegisterAsync()
        {
            // Gọi BLL để đăng ký tài khoản và đưa vào trạng thái Pending (async)
            try
            {
                var success = await _authService.RegisterAsync(Id, Password, "Reader", FullName, Gender).ConfigureAwait(false);

                // Marshal back to UI thread to show MessageBox
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (success)
                    {
                        MessageBox.Show("Đăng ký thành công. Tài khoản đang chờ admin duyệt.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Đăng ký thất bại: tài khoản đã tồn tại hoặc lỗi hệ thống.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
            catch (System.Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }
    }
}