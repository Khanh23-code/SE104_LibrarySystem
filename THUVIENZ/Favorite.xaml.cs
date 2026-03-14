using System.Windows;

namespace THUVIENZ
{
    public partial class Favorite : Window
    {
        public Favorite() { InitializeComponent(); }

        private void BtnProfile_Click(object sender, RoutedEventArgs e) { new Profile().Show(); this.Close(); }
        private void BtnSearch_Click(object sender, RoutedEventArgs e) { new Search().Show(); this.Close(); }
        private void BtnFine_Click(object sender, RoutedEventArgs e) { new Fine().Show(); this.Close(); }
        private void BtnRules_Click(object sender, RoutedEventArgs e) { new Rules().Show(); this.Close(); }
    }
}