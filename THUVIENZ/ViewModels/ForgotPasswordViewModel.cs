using THUVIENZ.Core;

namespace THUVIENZ.ViewModels
{
    public class ForgotPasswordViewModel : ObservableObject
    {
        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public ForgotPasswordViewModel()
        {
        }
    }
}