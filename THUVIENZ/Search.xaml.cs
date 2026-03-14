using System.Windows;

namespace THUVIENZ
{
    public partial class Search : Window
    {
        public Search() { InitializeComponent(); }

        private void BtnProfile_Click(object sender, RoutedEventArgs e) { new Profile().Show(); this.Close(); }
        private void BtnFavorite_Click(object sender, RoutedEventArgs e) { new Favorite().Show(); this.Close(); }
        private void BtnFine_Click(object sender, RoutedEventArgs e) { new Fine().Show(); this.Close(); }
        private void BtnRules_Click(object sender, RoutedEventArgs e) { new Rules().Show(); this.Close(); }
    }
}