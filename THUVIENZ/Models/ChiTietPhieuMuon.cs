using THUVIENZ.Core;

namespace THUVIENZ.Models
{
    public class ChiTietPhieuMuon : ObservableObject
    {
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

        private string? _trangThai;
        public string? TrangThai
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
