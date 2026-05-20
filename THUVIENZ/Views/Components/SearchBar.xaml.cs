using System.Windows;
using System.Windows.Controls;

namespace THUVIENZ.Views.Components
{
    public partial class SearchBar : UserControl
    {
        // Khai báo biến SearchText để Binding ra ngoài
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(SearchBar), new PropertyMetadata(string.Empty));

        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        public static readonly DependencyProperty SearchCommandProperty =
            DependencyProperty.Register("SearchCommand", typeof(System.Windows.Input.ICommand), typeof(SearchBar));

        public System.Windows.Input.ICommand SearchCommand
        {
            get { return (System.Windows.Input.ICommand)GetValue(SearchCommandProperty); }
            set { SetValue(SearchCommandProperty, value); }
        }

        public SearchBar()
        {
            InitializeComponent();
        }
    }
}