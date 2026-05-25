using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using THUVIENZ.BLL;
using THUVIENZ.Core;
using THUVIENZ.Models;
using THUVIENZ.DAL;
using Microsoft.EntityFrameworkCore;

namespace THUVIENZ.ViewModels
{
    /// <summary>
    /// ViewModel quản lý danh sách Độc giả trên giao diện Admin.
    /// Áp dụng Strict Null Safety tuyệt đối và chú thích Tiếng Việt.
    /// </summary>
    public class ReaderManagementViewModel : ObservableObject
    {
        private readonly ReaderManagementService _readerService;

        private ObservableCollection<DocGia> _readers = new ObservableCollection<DocGia>();
        public ObservableCollection<DocGia> Readers
        {
            get => _readers;
            set { _readers = value; OnPropertyChanged(); }
        }

        private int _pendingRequestCount;
        public int PendingRequestCount
        {
            get => _pendingRequestCount;
            set { _pendingRequestCount = value; OnPropertyChanged(); }
        }

        private string _searchKeyword = string.Empty;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                _searchKeyword = value;
                OnPropertyChanged();
                _ = ExecuteSearchAsync();
            }
        }

        private string _currentTabStatus = "Active";
        public string CurrentTabStatus
        {
            get => _currentTabStatus;
            set
            {
                _currentTabStatus = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsActiveTabVisibility));
                OnPropertyChanged(nameof(IsLockedTabVisibility));
                OnPropertyChanged(nameof(IsUnlockableTabVisibility));
                OnPropertyChanged(nameof(IsDeletableTabVisibility));
                _ = LoadDataAsync(); // Nạp lại ngay lập tức khi thay đổi trạng thái Tab
            }
        }

        public Visibility IsActiveTabVisibility => CurrentTabStatus == "Active" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsLockedTabVisibility => CurrentTabStatus == "Locked" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsUnlockableTabVisibility => (CurrentTabStatus == "Locked" || CurrentTabStatus == "DisActive") ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsDeletableTabVisibility => CurrentTabStatus != "DisActive" ? Visibility.Visible : Visibility.Collapsed;

        private int _activeCount;
        public int ActiveCount
        {
            get => _activeCount;
            set { _activeCount = value; OnPropertyChanged(); }
        }

        private int _lockedCount;
        public int LockedCount
        {
            get => _lockedCount;
            set { _lockedCount = value; OnPropertyChanged(); }
        }

        private int _disActiveCount;
        public int DisActiveCount
        {
            get => _disActiveCount;
            set { _disActiveCount = value; OnPropertyChanged(); }
        }

        // Bộ nhớ đệm lưu trữ vĩnh cửu lý do đình chỉ theo mã độc giả trong suốt phiên làm việc
        public static readonly System.Collections.Generic.Dictionary<int, string> SuspensionReasonsCache = new();

        public void SetTabStatus(string status)
        {
            CurrentTabStatus = status;
        }

        public ICommand LoadReadersCommand { get; }
        public ICommand DeleteReaderCommand { get; }
        public ICommand UnsuspendReaderCommand { get; }

        public ReaderManagementViewModel()
        {
            _readerService = new ReaderManagementService();

            LoadReadersCommand = new RelayCommand(async _ => await LoadDataAsync());
            DeleteReaderCommand = new RelayCommand(async param => await ExecuteDeleteAsync(param));
            UnsuspendReaderCommand = new RelayCommand(async param => await ExecuteUnsuspendAsync(param));

            _ = LoadDataAsync();
        }

        /// <summary>
        /// Nạp danh sách độc giả động dựa theo Trạng thái Tab hiện tại và Từ khóa tìm kiếm (Đảm bảo quy tắc DRY).
        /// </summary>
        public async Task LoadDataAsync()
        {
            try
            {
                using var context = new LmsDbContext();
                var query = context.DocGias.Include(d => d.TaiKhoan).AsQueryable();

                // Lọc theo phân hệ Tab Switcher
                if (CurrentTabStatus == "Active")
                {
                    query = query.Where(d => d.TaiKhoan == null || d.TaiKhoan.TrangThai == "Active");
                }
                else if (CurrentTabStatus == "Locked")
                {
                    query = query.Where(d => d.TaiKhoan != null && d.TaiKhoan.TrangThai == "Locked");
                }
                else if (CurrentTabStatus == "DisActive")
                {
                    query = query.Where(d => d.TaiKhoan != null && d.TaiKhoan.TrangThai == "DisActive");
                }

                // Lọc theo từ khóa tìm kiếm
                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    var keyword = SearchKeyword.ToLower();
                    query = query.Where(d => d.HoTen.ToLower().Contains(keyword) || 
                                             (d.Email != null && d.Email.ToLower().Contains(keyword)) ||
                                             (d.SoDienThoai != null && d.SoDienThoai.Contains(keyword)));
                }

                var readers = await query.ToListAsync();
                Readers = new ObservableCollection<DocGia>(readers);

                // Cập nhật thống kê số lượng tài khoản theo từng trạng thái
                ActiveCount = await context.DocGias.CountAsync(d => d.TaiKhoan == null || d.TaiKhoan.TrangThai == "Active");
                LockedCount = await context.DocGias.CountAsync(d => d.TaiKhoan != null && d.TaiKhoan.TrangThai == "Locked");
                DisActiveCount = await context.DocGias.CountAsync(d => d.TaiKhoan != null && d.TaiKhoan.TrangThai == "DisActive");

                PendingRequestCount = await context.TaiKhoans.CountAsync(t => t.TrangThai == "Pending");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách độc giả: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Tái sử dụng trọn vẹn logic nạp danh sách động để tuân thủ triệt để nguyên lý DRY.
        /// </summary>
        private async Task ExecuteSearchAsync()
        {
            await LoadDataAsync();
        }

        /// <summary>
        /// Thực hiện thủ tục vô hiệu hóa tài khoản độc giả (Soft Delete) để bảo toàn lịch sử mượn trả.
        /// </summary>
        private async Task ExecuteDeleteAsync(object? param)
        {
            if (param is DocGia reader)
            {
                var confirm = MessageBox.Show($"Bạn có chắc chắn muốn vô hiệu hóa tài khoản độc giả '{reader.HoTen}'?", 
                                              "Xác nhận vô hiệu hóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm == MessageBoxResult.No) return;

                try
                {
                    using var context = new LmsDbContext();
                    // Kiểm tra xem độc giả có cuốn sách vật lý nào đang mượn chưa trả không
                    bool hasActiveLoans = await context.ChiTietMuonTras
                        .Include(c => c.PhieuMuon)
                        .AnyAsync(c => c.PhieuMuon!.MaDocGia == reader.MaDocGia && c.NgayTraThucTe == null);

                    if (hasActiveLoans)
                    {
                        MessageBox.Show("Không thể vô hiệu hóa: Độc giả này vẫn còn sách đang mượn chưa hoàn trả.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Thực hiện Soft Delete: Không xóa vật lý để bảo toàn dữ liệu, chỉ đổi trạng thái tài khoản sang "DisActive"
                    if (!string.IsNullOrEmpty(reader.TenDangNhap))
                    {
                        var taiKhoan = await context.TaiKhoans.FindAsync(reader.TenDangNhap);
                        if (taiKhoan != null)
                        {
                            taiKhoan.TrangThai = "DisActive";
                        }
                    }
                    else
                    {
                        // Nếu độc giả chưa có tài khoản, tự sinh một tài khoản ảo trạng thái DisActive để lưu vết
                        var newTaiKhoan = new TaiKhoan
                        {
                            TenDangNhap = $"dis_{reader.MaDocGia}_{System.DateTime.Now:yyMMddHHmmss}",
                            MatKhau = "disactive",
                            Quyen = "Reader",
                            TrangThai = "DisActive"
                        };
                        context.TaiKhoans.Add(newTaiKhoan);
                        var targetReader = await context.DocGias.FindAsync(reader.MaDocGia);
                        if (targetReader != null) targetReader.TenDangNhap = newTaiKhoan.TenDangNhap;
                    }

                    await context.SaveChangesAsync();

                    MessageBox.Show("Đã vô hiệu hóa độc giả thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    MessageBox.Show($"Lỗi xử lý vô hiệu hóa từ CSDL: {errorMsg}\n\n(Gợi ý: Vui lòng kiểm tra lại ràng buộc CHECK constraint của bảng TAIKHOAN trong Database vật lý)", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Thực hiện thủ tục khôi phục/hủy đình chỉ trước thời hạn hoặc tái kích hoạt tài khoản độc giả bị vô hiệu hóa.
        /// </summary>
        private async Task ExecuteUnsuspendAsync(object? param)
        {
            if (param is DocGia reader)
            {
                string title = CurrentTabStatus == "DisActive" ? "Xác nhận khôi phục tài khoản" : "Xác nhận hủy đình chỉ";
                string msg = CurrentTabStatus == "DisActive"
                    ? $"Tài khoản độc giả '{reader.HoTen}' hiện đang bị vô hiệu hóa. Bạn có chắc chắn muốn khôi phục toàn bộ quyền hạn và kích hoạt lại tài khoản?"
                    : $"Bạn có chắc chắn muốn hủy đình chỉ trước thời hạn cho tài khoản độc giả '{reader.HoTen}' và khôi phục hoạt động bình thường?";

                var confirm = MessageBox.Show(msg, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm == MessageBoxResult.No) return;

                try
                {
                    using var context = new LmsDbContext();
                    if (!string.IsNullOrEmpty(reader.TenDangNhap))
                    {
                        var taiKhoan = await context.TaiKhoans.FindAsync(reader.TenDangNhap);
                        if (taiKhoan != null)
                        {
                            taiKhoan.TrangThai = "Active";
                        }
                    }
                    else
                    {
                        // Nếu tài khoản trước đó bị xóa hoàn toàn hoặc chưa tồn tại, tự động sinh lại tài khoản chuẩn hệ thống
                        var newTaiKhoan = new TaiKhoan
                        {
                            TenDangNhap = $"acc_{reader.MaDocGia}_{DateTime.Now:yyMMddHHmmss}",
                            MatKhau = "active_reloaded",
                            Quyen = "Reader",
                            TrangThai = "Active"
                        };
                        context.TaiKhoans.Add(newTaiKhoan);
                        var targetReader = await context.DocGias.FindAsync(reader.MaDocGia);
                        if (targetReader != null) targetReader.TenDangNhap = newTaiKhoan.TenDangNhap;
                    }

                    await context.SaveChangesAsync();

                    string successMsg = CurrentTabStatus == "DisActive" 
                        ? "Đã khôi phục và tái kích hoạt tài khoản độc giả thành công!" 
                        : "Đã khôi phục hoạt động tài khoản độc giả thành công!";
                    MessageBox.Show(successMsg, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xử lý khôi phục từ CSDL: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
