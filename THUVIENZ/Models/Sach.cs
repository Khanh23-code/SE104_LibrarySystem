using System;
using THUVIENZ.Core;

namespace THUVIENZ.Models
{
    public class Sach : ObservableObject
    {
        private int _maSach;
        public int MaSach
        {
            get => _maSach;
            set
            {
                _maSach = value;
                OnPropertyChanged();
            }
        }

        private string _tenSach = string.Empty;
        public string TenSach
        {
            get => _tenSach;
            set
            {
                _tenSach = value;
                OnPropertyChanged();
            }
        }

        private int? _maTheLoai;
        public int? MaTheLoai
        {
            get => _maTheLoai;
            set
            {
                _maTheLoai = value;
                OnPropertyChanged();
            }
        }

        private string? _tacGia;
        public string? TacGia
        {
            get => _tacGia;
            set
            {
                _tacGia = value;
                OnPropertyChanged();
            }
        }

        private int? _namXuatBan;
        public int? NamXuatBan
        {
            get => _namXuatBan;
            set
            {
                _namXuatBan = value;
                OnPropertyChanged();
            }
        }

        private string? _nhaXuatBan;
        public string? NhaXuatBan
        {
            get => _nhaXuatBan;
            set
            {
                _nhaXuatBan = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _ngayNhap;
        public DateTime? NgayNhap
        {
            get => _ngayNhap;
            set
            {
                _ngayNhap = value;
                OnPropertyChanged();
            }
        }

        private decimal? _triGia;
        public decimal? TriGia
        {
            get => _triGia;
            set
            {
                _triGia = value;
                OnPropertyChanged();
            }
        }

        private string? _tinhTrang;
        public string? TinhTrang
        {
            get => _tinhTrang;
            set
            {
                _tinhTrang = value;
                OnPropertyChanged();
            }
        }
    }
}
