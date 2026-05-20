using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using THUVIENZ.BLL;
using THUVIENZ.Core;
using THUVIENZ.Models;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using THUVIENZ.DAL;

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
        public ICommand BorrowCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }

        private readonly SearchService _searchService;

        public SearchViewModel()
        {
            _searchService = new SearchService();
            
            // Khởi tạo lệnh tìm kiếm
            SearchCommand = new RelayCommand(ExecuteSearch);
            BorrowCommand = new RelayCommand<Sach>(ExecuteBorrow);
            ToggleFavoriteCommand = new RelayCommand<Sach>(ExecuteToggleFavorite);
        }

        /// <summary>
        /// Thực hiện logic tìm kiếm và cập nhật UI thông qua ObservableCollection.
        /// </summary>
        public async void ExecuteSearch(object? parameter = null)
        {
            var books = (await _searchService.SearchAsync(SearchKeyword)).ToList();
            
            var username = UserSession.UserID;
            if (!string.IsNullOrEmpty(username))
            {
                using var context = new LmsDbContext();
                var docGia = await context.DocGias.AsNoTracking().FirstOrDefaultAsync(d => d.TenDangNhap == username);
                if (docGia != null)
                {
                    var favoriteBookIds = await context.SachYeuThichs
                        .AsNoTracking()
                        .Where(s => s.MaDocGia == docGia.MaDocGia)
                        .Select(s => s.MaSach)
                        .ToListAsync();
                        
                    foreach (var book in books)
                    {
                        book.IsFavorite = favoriteBookIds.Contains(book.MaSach);
                    }
                }
            }

            SearchResults = new ObservableCollection<Sach>(books);
        }

        private async void ExecuteBorrow(Sach sach)
        {
            if (sach == null) return;
            var username = UserSession.UserID;
            if (string.IsNullOrEmpty(username)) return;

            using var context = new LmsDbContext();
            var docGia = await context.DocGias.FirstOrDefaultAsync(d => d.TenDangNhap == username);
            if (docGia == null) return;

            var requestService = new THUVIENZ.BLL.RequestService();
            var success = await requestService.AddRequestAsync(docGia.MaDocGia, sach.MaSach);
            if (success)
            {
                MessageBox.Show("Mượn sách thành công! Yêu cầu đang chờ duyệt.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Sách này đã có trong danh sách yêu cầu mượn của bạn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void ExecuteToggleFavorite(Sach sach)
        {
            if (sach == null) return;
            var username = UserSession.UserID;
            if (string.IsNullOrEmpty(username)) return;

            using var context = new LmsDbContext();
            var docGia = await context.DocGias.FirstOrDefaultAsync(d => d.TenDangNhap == username);
            if (docGia == null) return;

            var favoriteService = new THUVIENZ.BLL.FavoriteService();
            // Since CheckBox binding updates sach.IsFavorite before the command executes, 
            // we can just match DB state to sach.IsFavorite
            if (sach.IsFavorite)
            {
                await favoriteService.AddFavoriteAsync(docGia.MaDocGia, sach.MaSach);
                // MessageBox.Show("Đã thêm vào mục yêu thích.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                await favoriteService.RemoveFavoriteAsync(docGia.MaDocGia, sach.MaSach);
                // MessageBox.Show("Đã gỡ khỏi mục yêu thích.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
