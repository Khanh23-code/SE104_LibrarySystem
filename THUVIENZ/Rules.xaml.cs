using System.Windows;

namespace THUVIENZ
{
    public partial class Rules : Window
    {
        public Rules() { InitializeComponent(); }

        private void BtnProfile_Click(object sender, RoutedEventArgs e) { new Profile().Show(); this.Close(); }
        private void BtnFavorite_Click(object sender, RoutedEventArgs e) { new Favorite().Show(); this.Close(); }
        private void BtnSearch_Click(object sender, RoutedEventArgs e) { new Search().Show(); this.Close(); }
        private void BtnFine_Click(object sender, RoutedEventArgs e) { new Fine().Show(); this.Close(); }
    }
}