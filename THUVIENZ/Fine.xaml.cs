using System.Windows;

namespace THUVIENZ
{
    public partial class Fine : Window
    {
        public Fine() { InitializeComponent(); }

        private void BtnProfile_Click(object sender, RoutedEventArgs e) { new Profile().Show(); this.Close(); }
        private void BtnFavorite_Click(object sender, RoutedEventArgs e) { new Favorite().Show(); this.Close(); }
        private void BtnSearch_Click(object sender, RoutedEventArgs e) { new Search().Show(); this.Close(); }
        private void BtnRules_Click(object sender, RoutedEventArgs e) { new Rules().Show(); this.Close(); }
    }
}