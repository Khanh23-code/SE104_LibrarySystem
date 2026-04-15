using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using THUVIENZ.BLL;
using THUVIENZ.DAL;
using THUVIENZ.Core;
using THUVIENZ.Models;

namespace THUVIENZ.ViewModels
{
    /// <summary>
    /// ViewModel xử lý logic mượn sách.
    /// Quản lý mã độc giả, giỏ hàng sách mượn và thực hiện giao dịch checkout.
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
            }
        }

        private string _bookIdInput = string.Empty;
        public string BookIdInput
        {
            get => _bookIdInput;
            set
            {
                _bookIdInput = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Sach> _cart = new ObservableCollection<Sach>();
        public ObservableCollection<Sach> Cart
        {
            get => _cart;
            set
            {
                _cart = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddToCartCommand { get; }
        public ICommand CheckoutCommand { get; }

        private readonly CirculationService _circulationService;
        private readonly SachRepository _sachRepository;

        public BorrowingViewModel()
        {
            _circulationService = new CirculationService();
            _sachRepository = new SachRepository();

            AddToCartCommand = new RelayCommand(_ => ExecuteAddToCart());
            CheckoutCommand = new RelayCommand(_ => ExecuteCheckout());
        }

        /// <summary>
        /// Tìm sách theo mã và thêm vào danh sách chờ mượn.
        /// </summary>
        private void ExecuteAddToCart()
        {
            if (int.TryParse(BookIdInput, out int bookId))
            {
                // Kiểm tra xem sách đã có trong giỏ hàng chưa
                if (Cart.Any(s => s.MaSach == bookId))
                {
                    MessageBox.Show("Cuốn sách này đã được thêm vào giỏ hàng mượn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Tìm kiếm sách trong CSDL
                var results = _sachRepository.SearchBooks(bookId.ToString());
                var book = results.FirstOrDefault(s => s.MaSach == bookId);

                if (book != null)
                {
                    if (book.TinhTrang == "Sẵn sàng")
                    {
                        Cart.Add(book);
                        BookIdInput = string.Empty; // Xóa input sau khi thêm thành công
                    }
                    else
                    {
                        MessageBox.Show($"Cuốn sách '{book.TenSach}' hiện đang ở trạng thái '{book.TinhTrang}', không thể mượn.", "Không khả dụng", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Không tìm thấy sách với mã số vừa nhập.", "Lỗi nạp sách", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Mã sách phải là một số nguyên hợp lệ.", "Lỗi định dạng", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Thực hiện lưu thông mượn sách thông qua Service.
        /// </summary>
        private void ExecuteCheckout()
        {
            if (ReaderId <= 0)
            {
                MessageBox.Show("Vui lòng nhập Mã độc giả trước khi thực hiện mượn.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Cart.Count == 0)
            {
                MessageBox.Show("Giỏ hàng đang trống. Vui lòng thêm sách cần mượn vào danh sách.", "Giỏ hàng trống", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Gọi service thực hiện mượn sách
            var result = _circulationService.BorrowBooks(ReaderId, Cart.Select(s => s.MaSach).ToList());

            if (result.Success)
            {
                MessageBox.Show(result.Message, "Mượn sách thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Reset form
                Cart.Clear();
                ReaderId = 0;
            }
            else
            {
                MessageBox.Show(result.Message, "Lỗi mượn sách", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
