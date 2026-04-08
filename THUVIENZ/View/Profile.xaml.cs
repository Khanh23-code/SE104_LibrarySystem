using System.Windows;

namespace THUVIENZ
{
    public partial class Profile : Window
    {
        public Profile() { InitializeComponent(); }

        private void BtnFavorite_Click(object sender, RoutedEventArgs e) { new Favorite().Show(); this.Close(); }
        private void BtnSearch_Click(object sender, RoutedEventArgs e) { new Search().Show(); this.Close(); }
        private void BtnBorrowing_Click(object sender, RoutedEventArgs e) { new Borrowing().Show(); this.Close(); }
        private void BtnNotifications_Click(object sender, RoutedEventArgs e) { new Notifications().Show(); this.Close(); }
        private void BtnLogout_Click(object sender, RoutedEventArgs e) { new Login().Show(); this.Close(); }
    }
}