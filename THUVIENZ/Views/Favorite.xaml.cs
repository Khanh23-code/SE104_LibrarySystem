using System.Windows;

namespace THUVIENZ.Views
{
    public partial class Favorite : Window
    {
        public Favorite() { InitializeComponent(); }

        private void BtnProfile_Click(object sender, RoutedEventArgs e) { new Profile().Show(); this.Close(); }
        private void BtnSearch_Click(object sender, RoutedEventArgs e) { new Search().Show(); this.Close(); }
        private void BtnBorrowing_Click(object sender, RoutedEventArgs e) { new Borrowing().Show(); this.Close(); }
        private void BtnNotifications_Click(object sender, RoutedEventArgs e) { new Notifications().Show(); this.Close(); }
        private void BtnLogout_Click(object sender, RoutedEventArgs e) { new Login().Show(); this.Close(); }
    }
}
