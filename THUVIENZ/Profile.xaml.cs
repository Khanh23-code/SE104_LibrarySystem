using System.Windows;

namespace THUVIENZ
{
    public partial class Profile : Window
    {
        public Profile() { InitializeComponent(); }

        private void BtnFavorite_Click(object sender, RoutedEventArgs e) { new Favorite().Show(); this.Close(); }
        private void BtnSearch_Click(object sender, RoutedEventArgs e) { new Search().Show(); this.Close(); }
        private void BtnFine_Click(object sender, RoutedEventArgs e) { new Fine().Show(); this.Close(); }
        private void BtnRules_Click(object sender, RoutedEventArgs e) { new Rules().Show(); this.Close(); }
        private void BtnLogout_Click(object sender, RoutedEventArgs e) { new Login().Show(); this.Close(); }
    }
}