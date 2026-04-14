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

        private void BtnFavorite_Click(object sender, RoutedEventArgs e) { new Favorite().Show(); this.Close(); }
        private void BtnSearch_Click(object sender, RoutedEventArgs e) { new Search().Show(); this.Close(); }
        private void BtnBorrowing_Click(object sender, RoutedEventArgs e) { new Borrowing().Show(); this.Close(); }
        private void BtnNotifications_Click(object sender, RoutedEventArgs e) { new Notifications().Show(); this.Close(); }
        private void BtnLogout_Click(object sender, RoutedEventArgs e) { new Login().Show(); this.Close(); }
    }
}
