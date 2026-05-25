using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using THUVIENZ.Models;

namespace THUVIENZ.Views.Popups
{
    public partial class AddBookPopup : UserControl
    {
        public event EventHandler<BookModel>? OnBookAdded;
        public event EventHandler<Sach>? OnBookUpdated;

        private string? _selectedImagePath;
        private bool _isEditMode = false;
        private int _editBookId = 0;
        private Sach? _editTargetBook;

        public AddBookPopup()
        {
            InitializeComponent();
        }

        public void OpenForAdd()
        {
            _isEditMode = false;
            _editBookId = 0;
            _editTargetBook = null;

            lblPopupTitle.Text = "Thêm Sách Mới";
            btnSaveContent.Content = "Thêm sách";

            txtBookID.Text = "";
            txtTitle.Text = "";
            txtAuthor.Text = "";
            cboCategory.SelectedCategoryId = 0;
            cboCategory.LoadCategories();
            txtLang.Text = "";
            txtPageNumber.Text = "";
            txtQuantity.Text = "1";
            txtStatus.Text = "Còn sách";
            spStatus.Visibility = Visibility.Collapsed;
            txtRealLang.Text = "Tiếng Việt";
            txtPrice.Text = "100.000";
            txtDescription.Text = "";

            _selectedImagePath = null;
            imgPreview.Source = null;
            spPlaceholder.Visibility = Visibility.Visible;
            tbImageName.Text = "Chọn ảnh...";
        }

        public void LoadBookForEdit(Sach book)
        {
            _isEditMode = true;
            _editBookId = book.MaSach;
            _editTargetBook = book;

            lblPopupTitle.Text = "Cập nhật Sách";
            btnSaveContent.Content = "Lưu thay đổi";

            txtBookID.Text = book.MaSach.ToString();
            txtTitle.Text = book.TenSach ?? "";
            txtAuthor.Text = book.TacGia ?? "";
            
            cboCategory.SelectedCategoryId = book.MaTheLoai ?? 1;
            cboCategory.LoadCategories();
            txtLang.Text = book.NhaXuatBan ?? "";
            txtPageNumber.Text = book.NamXuatBan.ToString();
            txtQuantity.Text = book.SoLuong.ToString();
            txtStatus.Text = book.TinhTrang ?? "Còn sách";
            spStatus.Visibility = Visibility.Visible;
            txtRealLang.Text = book.NgonNgu ?? "Tiếng Việt";
            txtPrice.Text = book.TriGia?.ToString("N0", new System.Globalization.CultureInfo("vi-VN")) ?? "100.000";
            txtDescription.Text = book.MoTa ?? "";

            _selectedImagePath = null;
            imgPreview.Source = null;
            spPlaceholder.Visibility = Visibility.Visible;
            tbImageName.Text = "Chọn ảnh mới (Tùy chọn)...";

            // Nạp lại ảnh bìa đã thiết lập từ trước (nếu có)
            if (book.HinhAnh != null && book.HinhAnh.Length > 0)
            {
                try
                {
                    var bitmap = new BitmapImage();
                    using (var mem = new MemoryStream(book.HinhAnh))
                    {
                        mem.Position = 0;
                        bitmap.BeginInit();
                        bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.UriSource = null;
                        bitmap.StreamSource = mem;
                        bitmap.EndInit();
                    }
                    bitmap.Freeze();

                    imgPreview.Source = bitmap;
                    spPlaceholder.Visibility = Visibility.Collapsed;
                    tbImageName.Text = "Ảnh bìa đã lưu";
                    _selectedImagePath = null;
                }
                catch { }
            }
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Chọn ảnh bìa sách",
                Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedImagePath = openFileDialog.FileName;
                
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(_selectedImagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    imgPreview.Source = bitmap;
                    spPlaceholder.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể hiển thị demo ảnh: {ex.Message}", "Lỗi tải ảnh", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                tbImageName.Text = Path.GetFileName(_selectedImagePath);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditMode && _editTargetBook != null)
            {
                LoadBookForEdit(_editTargetBook);
            }
            else
            {
                OpenForAdd();
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên sách.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTitle.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAuthor.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên tác giả.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAuthor.Focus();
                return;
            }

            if (cboCategory.SelectedCategoryId <= 0)
            {
                MessageBox.Show("Vui lòng chọn Thể loại sách.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_isEditMode && _editTargetBook != null)
            {
                _editTargetBook.TenSach = txtTitle.Text.Trim();
                _editTargetBook.TacGia = txtAuthor.Text.Trim();
                
                _editTargetBook.MaTheLoai = cboCategory.SelectedCategoryId;

                _editTargetBook.NhaXuatBan = txtLang.Text.Trim();
                
                if (int.TryParse(txtPageNumber.Text.Trim(), out int yearOrPage))
                    _editTargetBook.NamXuatBan = yearOrPage;

                if (int.TryParse(txtQuantity.Text.Trim(), out int qty))
                    _editTargetBook.SoLuong = qty;

                _editTargetBook.TinhTrang = string.IsNullOrWhiteSpace(txtStatus.Text) ? "Còn sách" : txtStatus.Text.Trim();
                _editTargetBook.NgonNgu = string.IsNullOrWhiteSpace(txtRealLang.Text) ? "Tiếng Việt" : txtRealLang.Text.Trim();
                string cleanPrice = txtPrice.Text.Replace(".", "").Replace(",", "").Trim();
                if (decimal.TryParse(cleanPrice, out decimal pEdit))
                    _editTargetBook.TriGia = pEdit;
                _editTargetBook.MoTa = txtDescription.Text.Trim();

                // Nếu có file ảnh bìa được chọn/đổi, thực hiện đọc byte[]
                if (!string.IsNullOrEmpty(_selectedImagePath) && File.Exists(_selectedImagePath))
                {
                    try
                    {
                        _editTargetBook.HinhAnh = File.ReadAllBytes(_selectedImagePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi đọc file ảnh: {ex.Message}");
                    }
                }

                OnBookUpdated?.Invoke(this, _editTargetBook);
            }
            else
            {
                int.TryParse(txtQuantity.Text.Trim(), out int qty);
                var newBook = new BookModel
                {
                    BookID = "NEW",
                    Title = txtTitle.Text.Trim(),
                    Author = txtAuthor.Text.Trim(),
                    Category = cboCategory.SelectedCategoryId.ToString(),
                    Language = string.IsNullOrWhiteSpace(txtLang.Text) ? "Tiếng Việt" : txtLang.Text.Trim(),
                    RealLanguage = string.IsNullOrWhiteSpace(txtRealLang.Text) ? "Tiếng Việt" : txtRealLang.Text.Trim(),
                    Price = decimal.TryParse(txtPrice.Text.Replace(".", "").Replace(",", "").Trim(), out decimal pAdd) ? pAdd : 100000,
                    PageNumber = string.IsNullOrWhiteSpace(txtPageNumber.Text) ? "2026" : txtPageNumber.Text.Trim(),
                    Quantity = qty > 0 ? qty : 1,
                    Status = string.IsNullOrWhiteSpace(txtStatus.Text) ? "Còn sách" : txtStatus.Text.Trim(),
                    Description = txtDescription.Text.Trim(),
                    ImagePath = _selectedImagePath
                };

                OnBookAdded?.Invoke(this, newBook);
            }

            this.Visibility = Visibility.Collapsed;
        }

        private bool _isFormattingPrice = false;
        private void TxtPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isFormattingPrice) return;

            string raw = txtPrice.Text.Replace(".", "").Replace(",", "").Trim();
            if (decimal.TryParse(raw, out decimal val))
            {
                _isFormattingPrice = true;
                int caret = txtPrice.CaretIndex;
                int origLen = txtPrice.Text.Length;

                txtPrice.Text = val.ToString("N0", new System.Globalization.CultureInfo("vi-VN"));

                int newLen = txtPrice.Text.Length;
                caret += (newLen - origLen);
                txtPrice.CaretIndex = Math.Max(0, Math.Min(newLen, caret));

                _isFormattingPrice = false;
            }
        }
    }

    public class BookModel
    {
        public string BookID { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string PageNumber { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RealLanguage { get; set; } = "Tiếng Việt";
        public decimal Price { get; set; } = 100000;
        public string? ImagePath { get; set; }
    }
}