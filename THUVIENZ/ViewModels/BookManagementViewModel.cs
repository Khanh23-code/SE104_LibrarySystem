using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using THUVIENZ.BLL;
using THUVIENZ.Core;
using THUVIENZ.Models;

namespace THUVIENZ.ViewModels
{
    /// <summary>
    /// ViewModel cho màn hình Quản lý Sách (Admin).
    /// Quản lý danh sách sách, sách đang chọn và các lệnh CRUD.
    /// </summary>
    public class BookManagementViewModel : ObservableObject
    {
        private ObservableCollection<Sach> _booksList = new ObservableCollection<Sach>();
        public ObservableCollection<Sach> BooksList
        {
            get => _booksList;
            set
            {
                _booksList = value;
                OnPropertyChanged();
            }
        }

        private Sach? _selectedBook;
        public Sach? SelectedBook
        {
            get => _selectedBook;
            set
            {
                _selectedBook = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadBooksCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        private readonly BookManagementService _bookService;

        public BookManagementViewModel()
        {
            _bookService = new BookManagementService();

            // Khởi tạo các câu lệnh (Commands)
            LoadBooksCommand = new RelayCommand(_ => LoadAllBooks());
            AddCommand = new RelayCommand(_ => ExecuteAdd());
            UpdateCommand = new RelayCommand(_ => ExecuteUpdate());
            DeleteCommand = new RelayCommand(_ => ExecuteDelete());

            // Tải dữ liệu ban đầu
            LoadAllBooks();
        }

        private void LoadAllBooks()
        {
            try
            {
                var books = _bookService.GetAllBooks();
                BooksList = new ObservableCollection<Sach>(books);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách sách: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAdd()
        {
            // Giả định SelectedBook đang chứa thông tin sách mới từ Form nhập liệu
            if (SelectedBook == null || string.IsNullOrWhiteSpace(SelectedBook.TenSach))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin sách mới.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_bookService.AddBook(SelectedBook))
                {
                    MessageBox.Show("Thêm sách thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAllBooks();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi nghiệp vụ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteUpdate()
        {
            if (SelectedBook == null)
            {
                MessageBox.Show("Vui lòng chọn một cuốn sách để cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_bookService.UpdateBook(SelectedBook))
                {
                    MessageBox.Show("Cập nhật thông tin sách thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAllBooks();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi nghiệp vụ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteDelete()
        {
            if (SelectedBook == null)
            {
                MessageBox.Show("Vui lòng chọn một cuốn sách để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa sách '{SelectedBook.TenSach}' (Mã: {SelectedBook.MaSach}) không?", 
                                        "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (confirm == MessageBoxResult.No) return;

            try
            {
                if (_bookService.DeleteBook(SelectedBook.MaSach))
                {
                    MessageBox.Show("Xóa sách thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAllBooks();
                }
            }
            catch (Exception ex)
            {
                // Hiển thị lỗi nghiệp vụ (ví dụ: Sách đang được mượn)
                MessageBox.Show(ex.Message, "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }
    }
}
