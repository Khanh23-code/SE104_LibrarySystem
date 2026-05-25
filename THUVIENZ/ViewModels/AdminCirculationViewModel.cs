using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using THUVIENZ.BLL;
using THUVIENZ.Core;
using THUVIENZ.DAL;
using THUVIENZ.Models;

namespace THUVIENZ.ViewModels
{
    /// <summary>
    /// ViewModel quản lý giao diện Circulation (Mượn/Trả) dành cho Admin.
    /// Kết nối trực tiếp với MuonTraService theo đúng cấu trúc DB chuẩn hóa mới.
    /// Áp dụng Strict Null Safety tuyệt đối và chú thích Tiếng Việt.
    /// </summary>
    public class AdminCirculationViewModel : ObservableObject
    {
        private readonly LmsDbContext _context;
        private readonly MuonTraService _muonTraService;
        private readonly LibrarySettingsService _settingsService;

        private int _selectedTabIndex = 0; // 0: Đang mượn, 1: Đang yêu cầu, 2: Lịch sử, 3: Quy định & Xử phạt
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCirculationVisible));
                OnPropertyChanged(nameof(IsSettingsVisible));

                if (_selectedTabIndex == 0 || _selectedTabIndex == 1 || _selectedTabIndex == 2)
                {
                    IsHistoryTab = (_selectedTabIndex == 2);
                    IsRequestTab = (_selectedTabIndex == 1);
                    BorrowedBooks.Clear();
                    SelectedReader = null;
                    _ = LoadReadersAsync();
                }
                else if (_selectedTabIndex == 3)
                {
                    _ = LoadSettingsAsync();
                }
            }
        }

        public Visibility IsCirculationVisible => SelectedTabIndex != 3 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsSettingsVisible => SelectedTabIndex == 3 ? Visibility.Visible : Visibility.Collapsed;

        // Các thuộc tính Quản lý Cơ chế / Tham số Thư viện
        private double _tienPhatMoiNgay;
        public double TienPhatMoiNgay
        {
            get => _tienPhatMoiNgay;
            set { _tienPhatMoiNgay = value; OnPropertyChanged(); }
        }

        private double _soNgayMuonToiDa;
        public double SoNgayMuonToiDa
        {
            get => _soNgayMuonToiDa;
            set { _soNgayMuonToiDa = value; OnPropertyChanged(); }
        }

        private double _soSachMuonToiDa;
        public double SoSachMuonToiDa
        {
            get => _soSachMuonToiDa;
            set { _soSachMuonToiDa = value; OnPropertyChanged(); }
        }

        private double _tongNoToiDa;
        public double TongNoToiDa
        {
            get => _tongNoToiDa;
            set { _tongNoToiDa = value; OnPropertyChanged(); }
        }

        private bool _isHistoryTab;
        public bool IsHistoryTab
        {
            get => _isHistoryTab;
            set
            {
                _isHistoryTab = value;
                OnPropertyChanged();
            }
        }

        private bool _isRequestTab;
        public bool IsRequestTab
        {
            get => _isRequestTab;
            set
            {
                _isRequestTab = value;
                OnPropertyChanged();
            }
        }

        private string _searchKeyword = string.Empty;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                _searchKeyword = value;
                OnPropertyChanged();
                _ = LoadReadersAsync();
            }
        }

        private ObservableCollection<DocGiaWithBorrowCount> _readersList = new ObservableCollection<DocGiaWithBorrowCount>();
        public ObservableCollection<DocGiaWithBorrowCount> ReadersList
        {
            get => _readersList;
            set { _readersList = value; OnPropertyChanged(); }
        }

        private DocGiaWithBorrowCount? _selectedReader;
        public DocGiaWithBorrowCount? SelectedReader
        {
            get => _selectedReader;
            set 
            { 
                _selectedReader = value; 
                OnPropertyChanged();
                if (_selectedReader != null) _ = LoadBorrowedBooksAsync(_selectedReader.MaDocGia);
            }
        }

        private ObservableCollection<BorrowedBookInfo> _borrowedBooks = new ObservableCollection<BorrowedBookInfo>();
        public ObservableCollection<BorrowedBookInfo> BorrowedBooks
        {
            get => _borrowedBooks;
            set { _borrowedBooks = value; OnPropertyChanged(); }
        }

        public ICommand LoadDataCommand { get; }
        public ICommand ReturnBookCommand { get; }
        public ICommand SaveSettingsCommand { get; }

        public AdminCirculationViewModel()
        {
            _context = new LmsDbContext();
            _muonTraService = new MuonTraService();
            _settingsService = new LibrarySettingsService();

            LoadDataCommand = new RelayCommand(async _ => await LoadReadersAsync());
            ReturnBookCommand = new RelayCommand(async param => await ExecuteReturnAsync(param));
            SaveSettingsCommand = new RelayCommand(async _ => await SaveSettingsAsync());

            _ = LoadReadersAsync();
        }

        /// <summary>
        /// Nạp dữ liệu các tham số cấu hình quy định thư viện.
        /// </summary>
        public async Task LoadSettingsAsync()
        {
            try
            {
                TienPhatMoiNgay = await _settingsService.GetValueAsync("TienPhatMoiNgay");
                SoNgayMuonToiDa = await _settingsService.GetValueAsync("SoNgayMuonToiDa");
                SoSachMuonToiDa = await _settingsService.GetValueAsync("SoSachMuonToiDa");

                try
                {
                    TongNoToiDa = await _settingsService.GetValueAsync("TongNoToiDa");
                }
                catch (KeyNotFoundException)
                {
                    TongNoToiDa = 50000;
                    await _settingsService.UpdateParamAsync("TongNoToiDa", 50000);
                }
            }
            catch (Exception)
            {
                // Xử lý mặc định an toàn nếu DB chưa seed đủ
            }
        }

        /// <summary>
        /// Lưu cấu hình tham số mới xuống DB và cập nhật Cache LRU.
        /// </summary>
        public async Task SaveSettingsAsync()
        {
            try
            {
                await _settingsService.UpdateParamAsync("TienPhatMoiNgay", TienPhatMoiNgay);
                await _settingsService.UpdateParamAsync("SoNgayMuonToiDa", SoNgayMuonToiDa);
                await _settingsService.UpdateParamAsync("SoSachMuonToiDa", SoSachMuonToiDa);
                await _settingsService.UpdateParamAsync("TongNoToiDa", TongNoToiDa);

                MessageBox.Show("Đã cập nhật chính sách và tham số xử phạt thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu tham số: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Nạp danh sách các độc giả đang có giao dịch tương ứng với Tab hiện tại, hỗ trợ lọc theo từ khóa.
        /// </summary>
        public async Task LoadReadersAsync()
        {
            bool isReturnedFilter = IsHistoryTab;
            string keyword = SearchKeyword?.Trim().ToLower() ?? "";

            // Lưu lại ID độc giả đang được chọn trước khi nạp lại danh sách
            int? currentSelectedId = _selectedReader?.MaDocGia;

            var query = _context.DocGias.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(d => d.HoTen.ToLower().Contains(keyword) || d.MaDocGia.ToString().Contains(keyword));
            }

            List<DocGiaWithBorrowCount> readers;
            if (IsRequestTab)
            {
                readers = await query
                    .Select(d => new DocGiaWithBorrowCount
                    {
                        MaDocGia = d.MaDocGia,
                        HoTen = d.HoTen,
                        BorrowCount = _context.YeuCauMuons
                            .Count(y => y.MaDocGia == d.MaDocGia && y.TrangThai == "Pending")
                    })
                    .Where(x => x.BorrowCount > 0)
                    .ToListAsync();
            }
            else
            {
                readers = await query
                    .Select(d => new DocGiaWithBorrowCount
                    {
                        MaDocGia = d.MaDocGia,
                        HoTen = d.HoTen,
                        BorrowCount = _context.ChiTietMuonTras
                            .Include(c => c.PhieuMuon)
                            .Count(c => c.PhieuMuon!.MaDocGia == d.MaDocGia && (isReturnedFilter ? c.NgayTraThucTe != null : c.NgayTraThucTe == null))
                    })
                    .Where(x => x.BorrowCount > 0)
                    .ToListAsync();
            }

            ReadersList = new ObservableCollection<DocGiaWithBorrowCount>(readers);

            // Tự động phục hồi lại chọn lựa độc giả hiện tại để người dùng có thể nhấp trả liên tiếp các cuốn sách
            if (currentSelectedId.HasValue)
            {
                var matchedReader = ReadersList.FirstOrDefault(r => r.MaDocGia == currentSelectedId.Value);
                if (matchedReader != null)
                {
                    SelectedReader = matchedReader;
                }
                else
                {
                    SelectedReader = null;
                    BorrowedBooks.Clear();
                }
            }
        }

        /// <summary>
        /// Nạp danh sách chi tiết các cuốn sách vật lý mượn/trả của độc giả được chọn.
        /// </summary>
        public async Task LoadBorrowedBooksAsync(int readerId)
        {
            if (IsRequestTab)
            {
                var requests = await _context.YeuCauMuons
                    .Include(y => y.Sach)
                    .Where(y => y.MaDocGia == readerId && y.TrangThai == "Pending")
                    .Select(y => new BorrowedBookInfo
                    {
                        MaSach = y.MaSach,
                        MaCuonSach = 0,
                        MaYeuCau = y.MaYeuCau,
                        TenSach = y.Sach != null ? y.Sach.TenSach : "Không xác định",
                        NgayMuon = y.NgayYeuCau,
                        HanTra = DateTime.Now,
                        NgayTra = null,
                        TienPhat = 0,
                        IsActionEnabled = true,
                        ActionText = "Phê duyệt"
                    })
                    .ToListAsync();

                BorrowedBooks = new ObservableCollection<BorrowedBookInfo>(requests);
            }
            else
            {
                bool isReturnedFilter = IsHistoryTab;

                var books = await _context.ChiTietMuonTras
                    .Include(c => c.PhieuMuon)
                    .Include(c => c.CuonSach)
                    .ThenInclude(cs => cs!.Sach)
                    .Where(c => c.PhieuMuon!.MaDocGia == readerId && (isReturnedFilter ? c.NgayTraThucTe != null : c.NgayTraThucTe == null))
                    .Select(c => new BorrowedBookInfo
                    {
                        MaSach = c.MaCuonSach, // Ánh xạ sang MaCuonSach để hiển thị đúng ID vật lý trên cột DataGrid XAML
                        MaCuonSach = c.MaCuonSach,
                        MaYeuCau = 0,
                        TenSach = c.CuonSach!.Sach != null ? c.CuonSach.Sach.TenSach : "Không xác định",
                        NgayMuon = c.PhieuMuon!.NgayMuon,
                        HanTra = c.HanTra,
                        NgayTra = c.NgayTraThucTe,
                        TienPhat = c.TienPhat,
                        IsActionEnabled = c.NgayTraThucTe == null,
                        ActionText = c.NgayTraThucTe == null ? "Nhận trả" : "Đã trả"
                    })
                    .ToListAsync();

                BorrowedBooks = new ObservableCollection<BorrowedBookInfo>(books);
            }
        }

        /// <summary>
        /// Thực hiện thủ tục nhận trả cuốn sách vật lý được chọn.
        /// </summary>
        private async Task ExecuteReturnAsync(object? param)
        {
            if (IsHistoryTab)
            {
                MessageBox.Show("Sách này đã được hoàn trả xong.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (param is BorrowedBookInfo book && SelectedReader != null)
            {
                int currentReaderId = SelectedReader.MaDocGia;
                
                if (IsRequestTab)
                {
                    // Thực hiện phê duyệt yêu cầu mượn sách
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        // 1. Tìm cuốn sách vật lý (CuonSach) bất kỳ của đầu sách này có TrangThai là "Sẵn sàng"
                        var cuonSach = await _context.CuonSachs
                            .FirstOrDefaultAsync(cs => cs.MaSach == book.MaSach && cs.TinhTrang == "Sẵn sàng");

                        if (cuonSach == null)
                        {
                            MessageBox.Show("Không có cuốn sách vật lý nào sẵn sàng cho mượn đối với đầu sách này.", "Không đủ sách vật lý", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // 2. Kiểm tra các quy định hạn mức mượn sách
                        int soSachToiDa = (int)await _settingsService.GetValueAsync("SoSachMuonToiDa");
                        int soNgayMuonToiDa = (int)await _settingsService.GetValueAsync("SoNgayMuonToiDa");

                        int soSachDangMuon = await _context.ChiTietMuonTras
                            .Include(c => c.PhieuMuon)
                            .CountAsync(c => c.PhieuMuon!.MaDocGia == currentReaderId && c.NgayTraThucTe == null);

                        if (soSachDangMuon >= soSachToiDa)
                        {
                            MessageBox.Show($"Độc giả đã mượn tối đa số sách cho phép ({soSachToiDa} cuốn).", "Không thể phê duyệt", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // 3. Tạo Phiếu Mượn và Chi Tiết Mượn Trả mới
                        var phieuMuon = new PhieuMuon
                        {
                            MaDocGia = currentReaderId,
                            NgayMuon = DateTime.Now
                        };
                        await _context.PhieuMuons.AddAsync(phieuMuon);
                        await _context.SaveChangesAsync();

                        var chiTiet = new ChiTietMuonTra
                        {
                            MaPhieuMuon = phieuMuon.MaPhieuMuon,
                            MaCuonSach = cuonSach.MaCuonSach,
                            HanTra = DateTime.Now.AddDays(soNgayMuonToiDa),
                            NgayTraThucTe = null,
                            TienPhat = 0
                        };
                        await _context.ChiTietMuonTras.AddAsync(chiTiet);

                        // Cập nhật trạng thái cuốn sách thành "Đang mượn"
                        cuonSach.TinhTrang = "Đang mượn";
                        _context.CuonSachs.Update(cuonSach);

                        // 4. Đánh dấu trạng thái yêu cầu mượn là "Notified"
                        var request = await _context.YeuCauMuons.FindAsync(book.MaYeuCau);
                        if (request != null)
                        {
                            request.TrangThai = "Notified";
                            _context.YeuCauMuons.Update(request);
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        MessageBox.Show("Phê duyệt yêu cầu mượn sách thành công!", "Hoàn tất phê duyệt", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Làm mới danh sách độc giả và sách
                        await LoadReadersAsync();
                        if (SelectedReader != null && SelectedReader.MaDocGia == currentReaderId)
                        {
                            await LoadBorrowedBooksAsync(currentReaderId);
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        MessageBox.Show($"Lỗi hệ thống khi phê duyệt: {ex.Message}", "Lỗi phê duyệt", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // Logic nhận trả sách bình thường
                    try
                    {
                        var ketQua = await _muonTraService.ThucHienTraSachAsync(book.MaCuonSach);
                        if (ketQua.ThanhCong)
                        {
                            MessageBox.Show(ketQua.ThongBao, "Hoàn tất trả sách", MessageBoxButton.OK, MessageBoxImage.Information);
                            
                            await LoadReadersAsync();
                            
                            if (SelectedReader != null && SelectedReader.MaDocGia == currentReaderId)
                            {
                                await LoadBorrowedBooksAsync(currentReaderId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi xử lý trả sách: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }

    public class DocGiaWithBorrowCount
    {
        public int MaDocGia { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public int BorrowCount { get; set; }
    }

    public class BorrowedBookInfo
    {
        public int MaSach { get; set; }
        public int MaCuonSach { get; set; }
        public int MaYeuCau { get; set; }
        public string TenSach { get; set; } = string.Empty;
        public DateTime NgayMuon { get; set; }
        public DateTime HanTra { get; set; }
        public DateTime? NgayTra { get; set; }
        public decimal TienPhat { get; set; }
        public bool IsActionEnabled { get; set; } = true;
        public string ActionText { get; set; } = "Nhận trả";
    }
}
