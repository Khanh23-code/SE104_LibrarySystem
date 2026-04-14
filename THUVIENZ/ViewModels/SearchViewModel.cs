using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using THUVIENZ.BLL;
using THUVIENZ.Core;
using THUVIENZ.Models;

namespace THUVIENZ.ViewModels
{
    /// <summary>
    /// ViewModel cho màn hình Tra cứu sách.
    /// Quản lý từ khóa tìm kiếm và danh sách kết quả trả về.
    /// </summary>
    public class SearchViewModel : ObservableObject
    {
        private string _searchKeyword = string.Empty;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                _searchKeyword = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Sach> _searchResults = new ObservableCollection<Sach>();
        public ObservableCollection<Sach> SearchResults
        {
            get => _searchResults;
            set
            {
                _searchResults = value;
                OnPropertyChanged();
            }
        }

        public ICommand SearchCommand { get; }

        private readonly SearchService _searchService;

        public SearchViewModel()
        {
            _searchService = new SearchService();
            
            // Khởi tạo lệnh tìm kiếm
            SearchCommand = new RelayCommand(ExecuteSearch);
        }

        /// <summary>
        /// Thực hiện logic tìm kiếm và cập nhật UI thông qua ObservableCollection.
        /// </summary>
        public void ExecuteSearch(object? parameter = null)
        {
            List<Sach> books = _searchService.Search(SearchKeyword);
            SearchResults = new ObservableCollection<Sach>(books);
        }
    }
}
