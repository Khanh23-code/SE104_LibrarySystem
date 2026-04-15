using System;
using THUVIENZ.Core;

namespace THUVIENZ.Models
{
    public class DocGia : ObservableObject
    {
        private int _maDocGia;
        public int MaDocGia
        {
            get => _maDocGia;
            set
            {
                _maDocGia = value;
                OnPropertyChanged();
            }
        }

        private string _hoTen = string.Empty;
        public string HoTen
        {
            get => _hoTen;
            set
            {
                _hoTen = value;
                OnPropertyChanged();
            }
        }

        private int? _maLoaiDocGia;
        public int? MaLoaiDocGia
        {
            get => _maLoaiDocGia;
            set
            {
                _maLoaiDocGia = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _ngaySinh;
        public DateTime? NgaySinh
        {
            get => _ngaySinh;
            set
            {
                _ngaySinh = value;
                OnPropertyChanged();
            }
        }

        private string? _diaChi;
        public string? DiaChi
        {
            get => _diaChi;
            set
            {
                _diaChi = value;
                OnPropertyChanged();
            }
        }

        private string? _email;
        public string? Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _ngayLapThe;
        public DateTime? NgayLapThe
        {
            get => _ngayLapThe;
            set
            {
                _ngayLapThe = value;
                OnPropertyChanged();
            }
        }

        private decimal? _tongNo;
        public decimal? TongNo
        {
            get => _tongNo;
            set
            {
                _tongNo = value;
                OnPropertyChanged();
            }
        }

        private string? _tenDangNhap;
        public string? TenDangNhap
        {
            get => _tenDangNhap;
            set
            {
                _tenDangNhap = value;
                OnPropertyChanged();
            }
        }
    }
}
