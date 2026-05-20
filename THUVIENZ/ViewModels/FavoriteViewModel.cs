using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using THUVIENZ.Core;
using System.Windows;
using THUVIENZ.DAL;
using THUVIENZ.Models;
using System.Windows.Input;

namespace THUVIENZ.ViewModels
{
    public class FavoriteViewModel : ObservableObject
    {
        // Do not hold a long-lived DbContext; create per-operation to avoid stale data
        private ObservableCollection<Sach> _favoriteBooks = new ObservableCollection<Sach>();
        public ObservableCollection<Sach> FavoriteBooks
        {
            get => _favoriteBooks;
            set
            {
                _favoriteBooks = value;
                OnPropertyChanged();
            }
        }

        public ICommand RemoveFavoriteCommand { get; }
        public ICommand BorrowCommand { get; }

        public FavoriteViewModel()
        {
            RemoveFavoriteCommand = new RelayCommand<Sach>(ExecuteRemoveFavorite);
            BorrowCommand = new RelayCommand<Sach>(ExecuteBorrow);
            // Subscribe to favorite changes so the view model stays in sync when favorites
            // are modified from other parts of the app (e.g., SearchViewModel).
            THUVIENZ.BLL.FavoriteService.FavoriteChanged += OnFavoriteChanged;

            // Ensure initial load if viewmodel constructed after login
            if (!string.IsNullOrEmpty(UserSession.UserID))
            {
                _ = LoadFavoriteBooksAsync(UserSession.UserID);
            }

            // Also listen for login events so we can load favorites when user logs in
            UserSession.UserLoggedIn += () =>
            {
                if (!string.IsNullOrEmpty(UserSession.UserID))
                {
                    _ = LoadFavoriteBooksAsync(UserSession.UserID);
                }
            };
        }

        private void OnFavoriteChanged(int maDocGia, int maSach, bool added)
        {
            // Only react for current logged-in user
            if (string.IsNullOrEmpty(UserSession.UserID)) return;
            var currentUsername = UserSession.UserID;
            // Simply reload the favorite list for the current user to ensure consistency
            _ = LoadFavoriteBooksAsync(currentUsername);
        }

        private async Task HandleFavoriteChangedAsync(string username, int maDocGia, int maSach, bool added)
        {
            using var context = new LmsDbContext();
            var docGia = await context.DocGias.FirstOrDefaultAsync(d => d.TenDangNhap == username);
            if (docGia == null) return;
            if (docGia.MaDocGia != maDocGia) return;

            if (added)
            {
                // Load the book and add to collection if not present
                var sach = await context.Sachs.FirstOrDefaultAsync(s => s.MaSach == maSach);
                if (sach != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (!FavoriteBooks.Any(f => f.MaSach == sach.MaSach))
                        {
                            FavoriteBooks.Add(sach);
                        }
                    });
                }
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var existing = FavoriteBooks.FirstOrDefault(f => f.MaSach == maSach);
                    if (existing != null)
                    {
                        FavoriteBooks.Remove(existing);
                    }
                });
            }
        }

        public async Task LoadFavoriteBooksAsync(string username)
        {
            using var context = new LmsDbContext();
            var docGia = await context.DocGias.FirstOrDefaultAsync(d => d.TenDangNhap == username);
            if (docGia == null) return;

            var sachYeuThichs = await context.SachYeuThichs
                .AsNoTracking()
                .Include(s => s.Sach)
                .Where(s => s.MaDocGia == docGia.MaDocGia)
                .Select(s => s.Sach!)
                .ToListAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                FavoriteBooks = new ObservableCollection<Sach>(sachYeuThichs);
            });
        }

        private async void ExecuteRemoveFavorite(Sach sach)
        {
            if (sach == null) return;

            var username = UserSession.UserID;
            if (string.IsNullOrEmpty(username)) return;

            using var context = new LmsDbContext();
            var docGia = await context.DocGias.FirstOrDefaultAsync(d => d.TenDangNhap == username);
            if (docGia == null) return;

            var fav = await context.SachYeuThichs.FirstOrDefaultAsync(s => s.MaDocGia == docGia.MaDocGia && s.MaSach == sach.MaSach);
            if (fav != null)
            {
                context.SachYeuThichs.Remove(fav);
                await context.SaveChangesAsync();
                Application.Current.Dispatcher.Invoke(() => FavoriteBooks.Remove(sach));
            }
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
    }
}
