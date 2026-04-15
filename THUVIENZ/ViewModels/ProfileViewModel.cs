using System.Collections.Generic;
using System.Collections.ObjectModel;
using THUVIENZ.BLL;
using THUVIENZ.Core;
using THUVIENZ.Models;

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
                _currentReader = value;
                OnPropertyChanged();
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

        public ProfileViewModel()
        {
            _profileService = new ProfileService();
        }

        /// <summary>
        /// Tải dữ liệu hồ sơ và danh sách sách đang mượn dựa trên tên đăng nhập.
        /// </summary>
        public void LoadProfileData(string username)
        {
            // 1. Lấy thông tin độc giả
            CurrentReader = _profileService.GetReaderInfo(username);
            
            // 2. Nếu tìm thấy độc giả, tải danh sách sách đang mượn
            if (CurrentReader != null)
            {
                List<Sach> books = _profileService.GetActiveBorrowedBooks(CurrentReader.MaDocGia);
                BorrowedBooks = new ObservableCollection<Sach>(books);
            }
        }
    }
}
