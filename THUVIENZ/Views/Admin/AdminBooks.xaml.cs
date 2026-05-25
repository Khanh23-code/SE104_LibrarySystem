using System.Windows;
using System.Windows.Controls;
using THUVIENZ.ViewModels;
using THUVIENZ.Views.Popups;

namespace THUVIENZ.Views
{
    public partial class AdminBooks : UserControl
    {
        private readonly BookManagementViewModel _viewModel;

        public AdminBooks()
        {
            InitializeComponent();
            
            _viewModel = new BookManagementViewModel();
            this.DataContext = _viewModel;

            AddPopup.OnBookAdded += AddPopup_OnBookAdded;
            AddPopup.OnBookUpdated += AddPopup_OnBookUpdated;
        }

        private void BtnShowAddPopup_Click(object sender, RoutedEventArgs e)
        {
            AddPopup.OpenForAdd();
            AddPopup.Visibility = Visibility.Visible;
        }

        private async void BtnEditRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is THUVIENZ.Models.Sach bookRow)
            {
                try
                {
                    // Load data mới nhất từ database theo yêu cầu
                    var service = new THUVIENZ.BLL.BookManagementService();
                    var latestBook = await service.GetByIdAsync(bookRow.MaSach);
                    if (latestBook != null)
                    {
                        AddPopup.LoadBookForEdit(latestBook);
                        AddPopup.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy thông tin sách trong cơ sở dữ liệu.", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Lỗi tải thông tin sách: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void AddPopup_OnBookAdded(object? sender, BookModel newBook)
        {
            try
            {
                var service = new THUVIENZ.BLL.BookManagementService();
                int.TryParse(newBook.PageNumber, out int year);
                var sach = new THUVIENZ.Models.Sach
                {
                    MaISBN = $"ISBN-{System.Guid.NewGuid().ToString().Substring(0, 8)}",
                    TenSach = newBook.Title,
                    TacGia = newBook.Author,
                    NhaXuatBan = newBook.Language,
                    NgonNgu = newBook.RealLanguage,
                    NamXuatBan = year > 0 ? year : System.DateTime.Now.Year,
                    SoLuong = newBook.Quantity > 0 ? newBook.Quantity : 1,
                    TinhTrang = string.IsNullOrWhiteSpace(newBook.Status) ? "Còn sách" : newBook.Status,
                    MoTa = newBook.Description,
                    TriGia = newBook.Price > 0 ? newBook.Price : 100000,
                    MaTheLoai = 1
                };

                // Nếu nhập mã thể loại là số
                if (int.TryParse(newBook.Category, out int catId))
                    sach.MaTheLoai = catId;

                if (!string.IsNullOrEmpty(newBook.ImagePath) && System.IO.File.Exists(newBook.ImagePath))
                {
                    try
                    {
                        sach.HinhAnh = System.IO.File.ReadAllBytes(newBook.ImagePath);
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Lỗi đọc file ảnh bìa: {ex.Message}");
                    }
                }

                await service.AddBookAsync(sach);

                MessageBox.Show("Thêm sách mới thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                _viewModel.LoadBooksCommand.Execute(null);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Lỗi xử lý thêm sách: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddPopup_OnBookUpdated(object? sender, THUVIENZ.Models.Sach updatedBook)
        {
            try
            {
                var service = new THUVIENZ.BLL.BookManagementService();
                await service.UpdateAsync(updatedBook);

                MessageBox.Show("Cập nhật thông tin sách thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                _viewModel.LoadBooksCommand.Execute(null);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Lỗi xử lý cập nhật sách: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}