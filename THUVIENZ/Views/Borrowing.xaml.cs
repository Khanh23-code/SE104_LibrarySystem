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
    }
}
