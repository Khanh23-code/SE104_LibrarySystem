using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using THUVIENZ.Core;
using System.Windows;
using THUVIENZ.DAL;
using THUVIENZ.BLL;
using THUVIENZ.Models;
using System.Windows.Input;

namespace THUVIENZ.ViewModels
{
    public class FavoriteViewModel : ObservableObject
    {
        // Do not hold a long-lived DbContext; create per-operation to avoid stale data
        private ObservableCollection<FavoriteBookItem> _favoriteBooks = new ObservableCollection<FavoriteBookItem>();
        public ObservableCollection<FavoriteBookItem> FavoriteBooks
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

        private readonly MuonTraService _muonTraService;

        public FavoriteViewModel()
        {
            _muonTraService = new MuonTraService();
            RemoveFavoriteCommand = new RelayCommand<FavoriteBookItem>(ExecuteRemoveFavorite);
            BorrowCommand = new RelayCommand<FavoriteBookItem>(ExecuteBorrow);
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
                    var item = new FavoriteBookItem
                    {
                        MaSach = sach.MaSach,
                        TenSach = sach.TenSach,
                        TacGia = sach.TacGia,
                        TriGia = sach.TriGia,
                        HinhAnh = sach.HinhAnh
                    };

                    // Determine if the current user already has an active loan for this book
                    bool hasActiveLoan = await context.ChiTietMuonTras
                        .Include(c => c.PhieuMuon)
                        .AnyAsync(c => c.PhieuMuon!.MaDocGia == maDocGia && c.CuonSach != null && c.CuonSach.MaSach == maSach && c.NgayTraThucTe == null);

                    if (hasActiveLoan)
                    {
                        item.IsBorrowEnabled = false;
                        item.BorrowButtonText = "Đã mượn";
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (!FavoriteBooks.Any(f => f.MaSach == item.MaSach))
                        {
                            FavoriteBooks.Add(item);
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

            var items = new System.Collections.Generic.List<FavoriteBookItem>();
            foreach (var sach in sachYeuThichs)
            {
                var item = new FavoriteBookItem
                {
                    MaSach = sach.MaSach,
                    TenSach = sach.TenSach,
                    TacGia = sach.TacGia,
                    TriGia = sach.TriGia,
                    HinhAnh = sach.HinhAnh
                };

                bool hasActiveLoan = await context.ChiTietMuonTras
                    .Include(c => c.PhieuMuon)
                    .AnyAsync(c => c.PhieuMuon!.MaDocGia == docGia.MaDocGia && c.CuonSach != null && c.CuonSach.MaSach == sach.MaSach && c.NgayTraThucTe == null);

                if (hasActiveLoan)
                {
                    item.IsBorrowEnabled = false;
                    item.BorrowButtonText = "Đã mượn";
                }

                items.Add(item);
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                FavoriteBooks = new ObservableCollection<FavoriteBookItem>(items);
            });
        }

        private async void ExecuteRemoveFavorite(FavoriteBookItem sach)
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
                Application.Current.Dispatcher.Invoke(() => FavoriteBooks.Remove(FavoriteBooks.FirstOrDefault(f => f.MaSach == sach.MaSach)));
            }
        }

        private async void ExecuteBorrow(FavoriteBookItem sach)
        {
            if (sach == null) return;
            var username = UserSession.UserID;
            if (string.IsNullOrEmpty(username)) return;

            using var context = new LmsDbContext();
            var docGia = await context.DocGias.FirstOrDefaultAsync(d => d.TenDangNhap == username);
            if (docGia == null) return;

            // If the book is already borrowed by this user, do nothing
            if (!sach.IsBorrowEnabled)
            {
                MessageBox.Show("Bạn đã mượn cuốn sách này.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Find an available physical copy for this MaSach
            var cuonSach = await context.CuonSachs.FirstOrDefaultAsync(c => c.MaSach == sach.MaSach && c.TinhTrang == "Sẵn sàng");
            if (cuonSach == null)
            {
                MessageBox.Show("Hiện không có bản sao sẵn sàng để mượn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var success = await _muonTraService.ThucHienMuonSachAsync(docGia.MaDocGia, new System.Collections.Generic.List<int> { cuonSach.MaCuonSach });
                if (success)
                {
                    // Remove any pending request entries for this user/book since we completed the loan
                    var pending = await context.YeuCauMuons
                        .Where(y => y.MaDocGia == docGia.MaDocGia && y.MaSach == sach.MaSach && y.TrangThai == "Pending")
                        .ToListAsync();
                    if (pending.Any())
                    {
                        context.YeuCauMuons.RemoveRange(pending);
                        await context.SaveChangesAsync();
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        sach.IsBorrowEnabled = false;
                        sach.BorrowButtonText = "Đã mượn";
                    });

                    MessageBox.Show("Mượn sách thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Không thể mượn sách: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
