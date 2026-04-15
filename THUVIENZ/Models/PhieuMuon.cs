using System;
using THUVIENZ.Core;

namespace THUVIENZ.Models
{
    public class PhieuMuon : ObservableObject
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

        private DateTime _ngayMuon;
        public DateTime NgayMuon
        {
            get => _ngayMuon;
            set
            {
                _ngayMuon = value;
                OnPropertyChanged();
            }
        }
    }
}
