using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using THUVIENZ.BLL;
using THUVIENZ.Core;
using THUVIENZ.Models;
using System.Windows.Input;
using THUVIENZ.Commands;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;

namespace THUVIENZ.ViewModels
{
    /// <summary>
    /// ViewModel cho màn hình hồ sơ độc giả.
    /// Cung cấp dữ liệu để UI có thể Binding (CurrentReader và BorrowedBooks).
    /// </summary>
    public class ProfileViewModel : ObservableObject
    {
        private DocGia? _currentReader;
        public DocGia? CurrentReader
        {
            get => _currentReader;
            set
            {
                if (_currentReader != null)
                {
                    _currentReader.PropertyChanged -= CurrentReader_PropertyChanged;
                }

                _currentReader = value;
                OnPropertyChanged();

                if (_currentReader != null)
                {
                    _currentReader.PropertyChanged += CurrentReader_PropertyChanged;
                }

                // Reset change tracking when a new reader is loaded
                HasChanges = false;
            }
        }

        private ObservableCollection<Sach> _borrowedBooks = new ObservableCollection<Sach>();
        public ObservableCollection<Sach> BorrowedBooks
        {
            get => _borrowedBooks;
            set
            {
                _borrowedBooks = value;
                OnPropertyChanged();
            }
        }

        private readonly ProfileService _profileService;
        public ICommand? SaveCommand { get; private set; }
        private bool _hasChanges;
        public bool HasChanges
        {
            get => _hasChanges;
            set
            {
                if (_hasChanges == value) return;
                _hasChanges = value;
                OnPropertyChanged();
                // Update command CanExecute state
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ProfileViewModel()
        {
            _profileService = new ProfileService();
            SaveCommand = new AsyncRelayCommand(SaveChangesAsync, () => HasChanges);
        }

        /// <summary>
        /// Tải dữ liệu hồ sơ và danh sách sách đang mượn dựa trên tên đăng nhập.
        /// </summary>
        public async Task LoadProfileDataAsync(string username)
        {
            // 1. Lấy thông tin độc giả
            CurrentReader = await _profileService.GetReaderInfoAsync(username);
            
            // 2. Nếu tìm thấy độc giả, tải danh sách sách đang mượn
            if (CurrentReader != null)
            {
                var books = await _profileService.GetActiveBorrowedBooksAsync(CurrentReader.MaDocGia);
                BorrowedBooks = new ObservableCollection<Sach>(books);
            }
        }

        private async Task SaveChangesAsync()
        {
            if (CurrentReader == null) return;
            var ok = await _profileService.UpdateReaderAsync(CurrentReader);
            if (ok)
            {
                HasChanges = false;
                try
                {
                    MessageBox.Show("Lưu thông tin thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch { }
            }
            else
            {
                try
                {
                    MessageBox.Show("Lưu thông tin thất bại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch { }
            }
        }

        private void CurrentReader_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Bất kỳ thay đổi nào trên CurrentReader đều đánh dấu có thay đổi để bật nút Lưu
            HasChanges = true;
        }
    }
}
