using System.Windows;
using THUVIENZ.ViewModels;

namespace THUVIENZ.Views
{
    public partial class Search : Window
    {
        private readonly SearchViewModel _viewModel;

        public Search()
        {
            InitializeComponent();

            // Khá»Ÿi táº¡o ViewModel vÃ  thiáº¿t láº­p DataContext
            _viewModel = new SearchViewModel();
            this.DataContext = _viewModel;

            // Táº£i danh sÃ¡ch sÃ¡ch máº·c Ä‘á»‹nh khi vá»«a má»Ÿ mÃ n hÃ¬nh
            _viewModel.ExecuteSearch();
        }
    }
}
