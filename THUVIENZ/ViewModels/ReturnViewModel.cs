using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using THUVIENZ.BLL;
using THUVIENZ.Core;
using THUVIENZ.DAL;

namespace THUVIENZ.ViewModels
{
    /// <summary>
    /// Model hỗ trợ hiển thị các bản ghi sách đang mượn để thực hiện trả.
    /// </summary>
    public class BorrowingDisplayModel : ObservableObject
    {
        public int MaPhieuMuon { get; set; }
        public DateTime NgayMuon { get; set; }
        public int MaSach { get; set; }
        public string? TenSach { get; set; }
    }

    /// <summary>
    /// ViewModel cho tính năng Trả sách và xử lý Tiền phạt.
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

        private readonly ReturnService _returnService;
        private readonly PhieuMuonRepository _phieuMuonRepository;

        public ReturnViewModel()
        {
            _returnService = new ReturnService();
            _phieuMuonRepository = new PhieuMuonRepository();

            SearchCommand = new RelayCommand(_ => ExecuteSearch());
            ReturnBookCommand = new RelayCommand(_ => ExecuteReturn());
        }

        /// <summary>
        /// Tìm kiếm các cuốn sách mà độc giả hiện đang mượn nhưng chưa trả.
        /// </summary>
        private void ExecuteSearch()
        {
            if (int.TryParse(ReaderIdInput, out int readerId))
            {
                // Gọi DAL để lấy danh sách mượn
                var rawBooks = _phieuMuonRepository.GetActiveBorrowings(readerId);
                
                BorrowedBooks.Clear();
                foreach (var item in rawBooks)
                {
                    BorrowedBooks.Add(new BorrowingDisplayModel
                    {
                        MaPhieuMuon = item.MaPhieuMuon,
                        NgayMuon = item.NgayMuon,
                        MaSach = item.MaSach,
                        TenSach = item.TenSach
                    });
                }

                if (BorrowedBooks.Count == 0)
                {
                    MessageBox.Show("Thư viện không tìm thấy cuốn sách nào đang được mượn bởi độc giả này.", "Thông tin", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Mã độc giả phải là một số nguyên hợp lệ.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Thực hiện thủ tục trả cuốn sách đang được chọn.
        /// </summary>
        private void ExecuteReturn()
        {
            if (SelectedBorrowing == null)
            {
                MessageBox.Show("Vui lòng chọn cuốn sách cần trả từ danh sách kết quả.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (int.TryParse(ReaderIdInput, out int readerId))
            {
                // Gọi Service thực hiện nghiệp vụ trả và phạt
                var result = _returnService.ReturnBook(
                    SelectedBorrowing.MaPhieuMuon,
                    SelectedBorrowing.MaSach,
                    readerId,
                    SelectedBorrowing.NgayMuon
                );

                if (result.Success)
                {
                    MessageBox.Show(result.Message, "Trả sách thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Tải lại danh sách sau khi trả thành công
                    ExecuteSearch();
                }
                else
                {
                    MessageBox.Show(result.Message, "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
