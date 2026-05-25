using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using THUVIENZ.BLL;
using THUVIENZ.Core;
using THUVIENZ.DAL;

namespace THUVIENZ.ViewModels
{
    /// <summary>
    /// Model hỗ trợ hiển thị các bản ghi sách vật lý đang mượn để thực hiện trả.
    /// </summary>
    public class BorrowingDisplayModel : ObservableObject
    {
        public int MaPhieuMuon { get; set; }
        public int MaCuonSach { get; set; }
        public int MaSach { get; set; }
        public string? TenSach { get; set; }
        public DateTime NgayMuon { get; set; }
        public DateTime HanTra { get; set; }
    }

    /// <summary>
    /// ViewModel cho tính năng Trả sách và xử lý Tiền phạt tập trung.
    /// Kết nối trực tiếp với MuonTraService theo chuẩn DB mới.
    /// Áp dụng Strict Null Safety tuyệt đối và chú thích Tiếng Việt.
    /// </summary>
    public class ReturnViewModel : ObservableObject
    {
        private string _readerIdInput = string.Empty;
        public string ReaderIdInput
        {
            get => _readerIdInput;
            set
            {
                _readerIdInput = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<BorrowingDisplayModel> _borrowedBooks = new ObservableCollection<BorrowingDisplayModel>();
        public ObservableCollection<BorrowingDisplayModel> BorrowedBooks
        {
            get => _borrowedBooks;
            set
            {
                _borrowedBooks = value;
                OnPropertyChanged();
            }
        }

        private BorrowingDisplayModel? _selectedBorrowing;
        public BorrowingDisplayModel? SelectedBorrowing
        {
            get => _selectedBorrowing;
            set
            {
                _selectedBorrowing = value;
                OnPropertyChanged();
            }
        }

        public ICommand SearchCommand { get; }
        public ICommand ReturnBookCommand { get; }

        private readonly MuonTraService _muonTraService;
 
        public ReturnViewModel()
        {
            _muonTraService = new MuonTraService();
 
            SearchCommand = new RelayCommand(_ => ExecuteSearch());
            ReturnBookCommand = new RelayCommand(_ => ExecuteReturn());
        }

        /// <summary>
        /// Tìm kiếm các cuốn sách vật lý mà độc giả hiện đang mượn nhưng chưa trả.
        /// </summary>
        private async void ExecuteSearch()
        {
            if (int.TryParse(ReaderIdInput.Trim(), out int readerId))
            {
                try
                {
                    using var context = new LmsDbContext();
                    var rawBooks = await context.ChiTietMuonTras
                        .Include(c => c.PhieuMuon)
                        .Include(c => c.CuonSach)
                        .ThenInclude(cs => cs!.Sach)
                        .Where(c => c.PhieuMuon!.MaDocGia == readerId && c.NgayTraThucTe == null)
                        .Select(c => new BorrowingDisplayModel
                        {
                            MaPhieuMuon = c.MaPhieuMuon,
                            MaCuonSach = c.MaCuonSach,
                            MaSach = c.CuonSach!.MaSach,
                            TenSach = c.CuonSach.Sach != null ? c.CuonSach.Sach.TenSach : "Không xác định",
                            NgayMuon = c.PhieuMuon!.NgayMuon,
                            HanTra = c.HanTra
                        })
                        .ToListAsync();

                    BorrowedBooks = new ObservableCollection<BorrowingDisplayModel>(rawBooks);

                    if (BorrowedBooks.Count == 0)
                    {
                        MessageBox.Show("Thư viện không tìm thấy cuốn sách vật lý nào đang được mượn bởi độc giả này.", 
                                        "Thông tin", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi truy xuất danh sách mượn: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Mã độc giả phải là một số nguyên hợp lệ.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Thực hiện thủ tục trả cuốn sách vật lý đang được chọn qua MuonTraService.
        /// </summary>
        private async void ExecuteReturn()
        {
            if (SelectedBorrowing == null)
            {
                MessageBox.Show("Vui lòng chọn cuốn sách cần trả từ danh sách kết quả.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var ketQua = await _muonTraService.ThucHienTraSachAsync(SelectedBorrowing.MaCuonSach);

                if (ketQua.ThanhCong)
                {
                    MessageBox.Show(ketQua.ThongBao, "Trả sách thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Tải lại danh sách sau khi trả thành công để cập nhật giao diện realtime
                    ExecuteSearch();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xử lý hoàn trả: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
