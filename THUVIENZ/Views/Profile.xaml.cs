using System.Windows;
using THUVIENZ.Core;
using THUVIENZ.ViewModels;

namespace THUVIENZ.Views
{
    public partial class Profile : Window
    {
        private readonly ProfileViewModel _viewModel;

        public Profile()
        {
            InitializeComponent();
            
            // Khá»Ÿi táº¡o ViewModel vÃ  thiáº¿t láº­p DataContext cho Binding
            _viewModel = new ProfileViewModel();
            this.DataContext = _viewModel;

            // Táº£i dá»¯ liá»‡u hồ sơ cá»§a Ä‘á»™c giáº£ Ä‘angÄƒng nháº­p
            if (!string.IsNullOrEmpty(UserSession.Username))
            {
                _viewModel.LoadProfileData(UserSession.Username);
            }
        }
    }
}
