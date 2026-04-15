using THUVIENZ.Core;

namespace THUVIENZ.Models
{
    public class ChiTietPhieuTra : ObservableObject
    {
        private int _maPhieuTra;
        public int MaPhieuTra
        {
            get => _maPhieuTra;
            set
            {
                _maPhieuTra = value;
                OnPropertyChanged();
            }
        }

        private int _maPhieuMuon;
        public int MaPhieuMuon
        {
            get => _maPhieuMuon;
            set
            {
                _maPhieuMuon = value;
                OnPropertyChanged();
            }
        }

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

        private int? _soNgayMuon;
        public int? SoNgayMuon
        {
            get => _soNgayMuon;
            set
            {
                _soNgayMuon = value;
                OnPropertyChanged();
            }
        }

        private decimal? _tienPhat;
        public decimal? TienPhat
        {
            get => _tienPhat;
            set
            {
                _tienPhat = value;
                OnPropertyChanged();
            }
        }
    }
}
