using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using THUVIENZ.BLL;
using THUVIENZ.Core;
using THUVIENZ.DAL;
using THUVIENZ.Models;

namespace THUVIENZ.ViewModels
{
    /// <summary>
    /// Lớp hỗ trợ hiển thị danh sách sách đang mượn của cá nhân trên giao diện Borrowing.xaml.
    /// </summary>
    public class MyBorrowedBookItem
    {
        public string TicketID { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public string BorrowDate { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }



    /// <summary>
    /// ViewModel xử lý logic mượn sách vật lý tại Kiosk/Quầy tự phục vụ.
    /// Kết nối trực tiếp với MuonTraService theo đúng cấu trúc DB mới.
    /// Áp dụng Strict Null Safety tuyệt đối và chú thích Tiếng Việt.
    /// </summary>
    public class BorrowingViewModel : ObservableObject
    {
        private int _readerId;
        public int ReaderId
        {
            get => _readerId;
            set
            {
                _readerId = value;
                OnPropertyChanged();
                LoadMyBorrowedBooks();
            }
        }

        private string _bookIdInput = string.Empty;
        /// <summary>
        /// Chuỗi nhập liệu từ máy quét mã vạch/RFID (Mã cuốn sách vật lý).
        /// </summary>
        public string BookIdInput
        {
            get => _bookIdInput;
            set
            {
                _bookIdInput = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<CuonSach> _cart = new ObservableCollection<CuonSach>();
        /// <summary>
        /// Giỏ hàng chứa các cuốn sách vật lý chuẩn bị mượn.
        /// </summary>
        public ObservableCollection<CuonSach> Cart
        {
            get => _cart;
            set
            {
                _cart = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<MyBorrowedBookItem> _myBorrowedBooks = new ObservableCollection<MyBorrowedBookItem>();
        /// <summary>
        /// Danh sách sách đang mượn của độc giả (Hỗ trợ UI binding cho Borrowing.xaml).
        /// </summary>
        public ObservableCollection<MyBorrowedBookItem> MyBorrowedBooks
        {
            get => _myBorrowedBooks;
            set
            {
                _myBorrowedBooks = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddToCartCommand { get; }
        public ICommand CheckoutCommand { get; }
        public ICommand GiaHanCommand { get; }
        public ICommand TraSachCommand { get; }

        private readonly MuonTraService _muonTraService;
        private readonly LmsDbContext _context;

        public BorrowingViewModel()
        {
            _muonTraService = new MuonTraService();
            _context = new LmsDbContext();

            AddToCartCommand = new RelayCommand(_ => ExecuteAddToCart());
            CheckoutCommand = new RelayCommand(_ => ExecuteCheckout());
            GiaHanCommand = new RelayCommand<MyBorrowedBookItem>(item => _ = ExecuteGiaHanAsync(item));
            TraSachCommand = new RelayCommand<MyBorrowedBookItem>(item => _ = ExecuteTraSachAsync(item));
        }

        private async Task ExecuteGiaHanAsync(MyBorrowedBookItem item)
        {
            if (item == null) return;
            var parts = item.TicketID?.Split('-');
            if (parts == null || parts.Length < 3) return;
            if (!int.TryParse(parts[2], out int maCuonSach)) return;

            try
            {
                bool ok = await _muonTraService.GiaHanSachAsync(maCuonSach);
                if (ok)
                {
                    MessageBox.Show("Gia hạn thành công.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadMyBorrowedBooks();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gia hạn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExecuteTraSachAsync(MyBorrowedBookItem item)
        {
            if (item == null) return;
            var parts = item.TicketID?.Split('-');
            if (parts == null || parts.Length < 3) return;
            if (!int.TryParse(parts[2], out int maCuonSach)) return;

            try
            {
                var result = await _muonTraService.ThucHienTraSachAsync(maCuonSach);
                if (result.ThanhCong)
                {
                    MessageBox.Show(result.ThongBao, "Trả sách", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadMyBorrowedBooks();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi trả sách: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Nạp thông tin sách đang mượn của cá nhân để hiển thị realtime.
        /// </summary>
        public async void LoadMyBorrowedBooks()
        {
            if (ReaderId <= 0)
            {
                MyBorrowedBooks.Clear();
                return;
            }
            try
            {
                var list = await _context.ChiTietMuonTras
                    .Include(c => c.PhieuMuon)
                    .Include(c => c.CuonSach)
                    .ThenInclude(cs => cs!.Sach)
                    .Where(c => c.PhieuMuon!.MaDocGia == ReaderId)
                    .OrderByDescending(c => c.PhieuMuon!.NgayMuon)
                    .Select(c => new MyBorrowedBookItem
                    {
                        TicketID = $"PM-{c.MaPhieuMuon}-{c.MaCuonSach}",
                        BookTitle = c.CuonSach!.Sach != null ? c.CuonSach.Sach.TenSach : "Sách không xác định",
                        BorrowDate = c.PhieuMuon!.NgayMuon.ToString("dd/MM/yyyy"),
                        DueDate = c.HanTra.ToString("dd/MM/yyyy"),
                        Status = c.NgayTraThucTe == null ? (c.HanTra < DateTime.Now ? "Quá hạn" : "Đang mượn") : "Đã trả"
                    })
                    .ToListAsync();

                MyBorrowedBooks = new ObservableCollection<MyBorrowedBookItem>(list);
            }
            catch
            {
                // Bỏ qua lỗi nạp dữ liệu nền
            }
        }

        /// <summary>
        /// Xử lý quét mã vạch/RFID cuốn sách vật lý và thêm vào giỏ hàng.
        /// </summary>
        private async void ExecuteAddToCart()
        {
            if (int.TryParse(BookIdInput.Trim(), out int maCuonSach))
            {
                if (Cart.Any(c => c.MaCuonSach == maCuonSach))
                {
                    MessageBox.Show("Cuốn sách vật lý này đã có trong giỏ hàng mượn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var cuonSach = await _context.CuonSachs
                        .Include(c => c.Sach)
                        .FirstOrDefaultAsync(c => c.MaCuonSach == maCuonSach);

                    if (cuonSach != null)
                    {
                        if (cuonSach.TinhTrang == "Sẵn sàng")
                        {
                            Cart.Add(cuonSach);
                            BookIdInput = string.Empty;

                            // Kích hoạt cập nhật giao diện realtime cho thuộc tính NotMapped của Đầu Sách
                            if (cuonSach.Sach != null)
                            {
                                cuonSach.Sach.RaisePropertyChanged(nameof(Sach.SoLuong));
                                cuonSach.Sach.RaisePropertyChanged(nameof(Sach.TinhTrang));
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Cuốn sách '{cuonSach.Sach?.TenSach}' (Mã vật lý: {maCuonSach}) hiện đang ở tình trạng '{cuonSach.TinhTrang}', không thể mượn.", 
                                            "Không khả dụng", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Hệ thống không tìm thấy mã sách vật lý '{maCuonSach}'. Vui lòng thử quét lại.", 
                                        "Lỗi tìm kiếm", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi truy xuất cơ sở dữ liệu: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Mã cuốn sách phải là số nguyên hợp lệ từ tem RFID/Barcode.", "Định dạng không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Tiến hành thủ tục mượn sách tập trung qua MuonTraService.
        /// </summary>
        private async void ExecuteCheckout()
        {
            if (ReaderId <= 0)
            {
                MessageBox.Show("Vui lòng nhập hoặc quét Mã Độc Giả trước khi xác nhận mượn.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Cart.Count == 0)
            {
                MessageBox.Show("Giỏ hàng mượn đang trống. Vui lòng quét ít nhất một cuốn sách.", "Giỏ hàng trống", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var dsMaCuonSach = Cart.Select(c => c.MaCuonSach).ToList();
                bool thanhCong = await _muonTraService.ThucHienMuonSachAsync(ReaderId, dsMaCuonSach);

                if (thanhCong)
                {
                    MessageBox.Show("Hoàn tất thủ tục mượn sách thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    Cart.Clear();
                    LoadMyBorrowedBooks(); // Làm mới danh sách hiển thị realtime
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Từ chối giao dịch", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
