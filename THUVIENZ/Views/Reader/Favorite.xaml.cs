using System.Windows.Controls;
using THUVIENZ.ViewModels;

namespace THUVIENZ.Views
{
    public partial class Favorite : UserControl
    {
        public Favorite()
        {
            InitializeComponent();
            this.DataContext = new FavoriteViewModel();
        }
    }
}
