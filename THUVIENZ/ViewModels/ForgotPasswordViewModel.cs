using System.Windows.Input;
using THUVIENZ.Core;

namespace THUVIENZ.ViewModels
{
    public class ForgotPasswordViewModel : ObservableObject
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
                _idError = value;

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

                if (string .IsNullOrEmpty(_password))
                    PasswordError = "Mật khẩu không được để trống.";
                else if (!InputValidator.IsValidPassword(_password))
                    PasswordError = "Mật khẩu phải từ 6 ký tự và không có khoảng trắng.";
                else if (!InputValidator.IsPasswordMatch(_password, ConfirmPassword))
                    PasswordError = "Mật khẩu xác nhận không khớp.";
                else
                    PasswordError = string.Empty;

                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested(); // Cập nhật trạng thái của các command liên quan nếu có
            }
        }

        private string _confirmPassword = string.Empty;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        public ICommand ForgotPasswordCommand { get; }

        public ForgotPasswordViewModel()
        {
            ForgotPasswordCommand = new RelayCommand(ExecuteForgotPassword, CanExecuteForgotPassword);
        }

        private void ExecuteForgotPassword(object parameter)
        {
            // Thực hiện logic quên mật khẩu ở đây
        }

        private bool CanExecuteForgotPassword(object parameter)
        {
            if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(Password))
                return false;
            if (!InputValidator.IsValidId(Id) || !InputValidator.IsValidPassword(Password))
                return false;
            if (!InputValidator.IsPasswordMatch(Password, ConfirmPassword))
                return false;
            return true;
        }
    }
}