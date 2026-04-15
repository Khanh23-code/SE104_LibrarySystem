using THUVIENZ.Core;

namespace THUVIENZ.Models
{
    public class TaiKhoan : ObservableObject
    {
        private string _tenDangNhap = string.Empty;
        public string TenDangNhap
        {
            get => _tenDangNhap;
            set
            {
                _tenDangNhap = value;
                OnPropertyChanged();
            }
        }

        private string _matKhau = string.Empty;
        public string MatKhau
        {
            get => _matKhau;
            set
            {
                _matKhau = value;
                OnPropertyChanged();
            }
        }

        private string _quyen = string.Empty;
        public string Quyen
        {
            get => _quyen;
            set
            {
                _quyen = value;
                OnPropertyChanged();
            }
        }

        private string _trangThai = "Pending";
        public string TrangThai
        {
            get => _trangThai;
            set
            {
                _trangThai = value;
                OnPropertyChanged();
            }
        }
    }
}
