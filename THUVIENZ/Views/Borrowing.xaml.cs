using System.Windows;
using THUVIENZ.ViewModels;

namespace THUVIENZ.Views
{
    public partial class Borrowing : Window
    {
        private readonly BorrowingViewModel _viewModel;

        public Borrowing()
        {
            InitializeComponent();

            // Khá»Ÿi táº¡o ViewModel vÃ  thiáº¿t láº­p DataContext cho Binding
            _viewModel = new BorrowingViewModel();
            this.DataContext = _viewModel;
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e) { new Profile().Show(); this.Close(); }
        private void BtnFavorite_Click(object sender, RoutedEventArgs e) { new Favorite().Show(); this.Close(); }
        private void BtnSearch_Click(object sender, RoutedEventArgs e) { new Search().Show(); this.Close(); }
        private void BtnNotifications_Click(object sender, RoutedEventArgs e) { new Notifications().Show(); this.Close(); }
    }
}
