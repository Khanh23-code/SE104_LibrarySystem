using System.Windows;

namespace THUVIENZ.Views
{
    public partial class Notifications : Window
    {
        public Notifications() { InitializeComponent(); }

        private void BtnProfile_Click(object sender, RoutedEventArgs e) { new Profile().Show(); this.Close(); }
        private void BtnFavorite_Click(object sender, RoutedEventArgs e) { new Favorite().Show(); this.Close(); }
        private void BtnSearch_Click(object sender, RoutedEventArgs e) { new Search().Show(); this.Close(); }
        private void BtnBorrowing_Click(object sender, RoutedEventArgs e) { new Borrowing().Show(); this.Close(); }
    }
}
