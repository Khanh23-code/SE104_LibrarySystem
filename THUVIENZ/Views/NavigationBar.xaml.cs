using System.Windows;
using System.Windows.Controls;
using THUVIENZ.Views;

namespace THUVIENZ
{
    public partial class NavigationBar : UserControl
    {
        public NavigationBar()
        {
            InitializeComponent();
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e) { if (!(Window.GetWindow(this) is Profile)) { new Profile().Show(); Window.GetWindow(this)?.Close(); } }
        private void BtnFavorite_Click(object sender, RoutedEventArgs e) { if (!(Window.GetWindow(this) is Favorite)) { new Favorite().Show(); Window.GetWindow(this)?.Close(); } }
        private void BtnSearch_Click(object sender, RoutedEventArgs e) { if (!(Window.GetWindow(this) is Search)) { new Search().Show(); Window.GetWindow(this)?.Close(); } }
        private void BtnBorrowing_Click(object sender, RoutedEventArgs e) { if (!(Window.GetWindow(this) is Borrowing)) { new Borrowing().Show(); Window.GetWindow(this)?.Close(); } }
        private void BtnNotifications_Click(object sender, RoutedEventArgs e) { if (!(Window.GetWindow(this) is Notifications)) { new Notifications().Show(); Window.GetWindow(this)?.Close(); } }
        private void BtnLogout_Click(object sender, RoutedEventArgs e) { new Login().Show(); Window.GetWindow(this)?.Close(); }
    }
}
