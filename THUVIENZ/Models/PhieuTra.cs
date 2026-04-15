using System;
using THUVIENZ.Core;

namespace THUVIENZ.Models
{
    public class PhieuTra : ObservableObject
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

        private DateTime _ngayTra;
        public DateTime NgayTra
        {
            get => _ngayTra;
            set
            {
                _ngayTra = value;
                OnPropertyChanged();
            }
        }

        private decimal? _tienPhatKyNay;
        public decimal? TienPhatKyNay
        {
            get => _tienPhatKyNay;
            set
            {
                _tienPhatKyNay = value;
                OnPropertyChanged();
            }
        }
    }
}
