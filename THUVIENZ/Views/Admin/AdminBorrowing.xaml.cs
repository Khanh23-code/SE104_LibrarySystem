using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using THUVIENZ.ViewModels;

namespace THUVIENZ.Views
{
    public partial class AdminBorrowing : UserControl
    {
        private readonly AdminCirculationViewModel _viewModel;
        private readonly Border?[] _tabBorders = new Border?[4];

        public AdminBorrowing()
        {
            InitializeComponent();
            _viewModel = new AdminCirculationViewModel();
            this.DataContext = _viewModel;
            this.Loaded += AdminBorrowing_Loaded;
        }

        private void AdminBorrowing_Loaded(object sender, RoutedEventArgs e)
        {
            FindTabBorders(this);
            UpdateTabStyles();
        }

        private void FindTabBorders(DependencyObject parent)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is Border b && b.Child is TextBlock tb)
                {
                    if (tb.Text.Contains("Đang mượn"))
                    {
                        RegisterTab(b, 0);
                    }
                    else if (tb.Text.Contains("Đang yêu cầu"))
                    {
                        RegisterTab(b, 1);
                    }
                    else if (tb.Text.Contains("Lịch sử"))
                    {
                        RegisterTab(b, 2);
                    }
                    else if (tb.Text.Contains("Quy định"))
                    {
                        RegisterTab(b, 3);
                    }
                }
                else
                {
                    FindTabBorders(child);
                }
            }
        }

        private void RegisterTab(Border border, int index)
        {
            _tabBorders[index] = border;
            border.MouseLeftButtonDown -= Tab_Click;
            border.MouseLeftButtonDown += Tab_Click;
        }

        private void Tab_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                for (int i = 0; i < _tabBorders.Length; i++)
                {
                    if (_tabBorders[i] == border)
                    {
                        _viewModel.SelectedTabIndex = i;
                        break;
                    }
                }
                UpdateTabStyles();
            }
        }

        private void UpdateTabStyles()
        {
            for (int i = 0; i < _tabBorders.Length; i++)
            {
                var border = _tabBorders[i];
                if (border?.Child is TextBlock tb)
                {
                    bool isSelected = (_viewModel.SelectedTabIndex == i);
                    border.Background = isSelected ? new SolidColorBrush(Color.FromRgb(239, 246, 255)) : new SolidColorBrush(Colors.Transparent);
                    border.BorderBrush = isSelected ? new SolidColorBrush(Color.FromRgb(59, 130, 246)) : new SolidColorBrush(Colors.Transparent);
                    tb.Foreground = isSelected ? new SolidColorBrush(Color.FromRgb(29, 78, 216)) : new SolidColorBrush(Color.FromRgb(107, 114, 128));
                    tb.FontWeight = isSelected ? FontWeights.Bold : FontWeights.SemiBold;
                }
            }
        }

        private void NumberOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]+$");
        }
    }
}