using System;
using THUVIENZ.Core;

namespace THUVIENZ.Models
{
    public class PhieuThuTienPhat : ObservableObject
    {
        private int _maPhieuThu;
        public int MaPhieuThu
        {
            get => _maPhieuThu;
            set
            {
                _maPhieuThu = value;
                OnPropertyChanged();
            }
        }

        private int? _maDocGia;
        public int? MaDocGia
        {
            get => _maDocGia;
            set
            {
                _maDocGia = value;
                OnPropertyChanged();
            }
        }

        private decimal _soTienThu;
        public decimal SoTienThu
        {
            get => _soTienThu;
            set
            {
                _soTienThu = value;
                OnPropertyChanged();
            }
        }

        private decimal? _conLai;
        public decimal? ConLai
        {
            get => _conLai;
            set
            {
                _conLai = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _ngayThu;
        public DateTime? NgayThu
        {
            get => _ngayThu;
            set
            {
                _ngayThu = value;
                OnPropertyChanged();
            }
        }
    }
}
