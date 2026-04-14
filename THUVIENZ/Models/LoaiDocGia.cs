using THUVIENZ.Core;

namespace THUVIENZ.Models
{
    public class LoaiDocGia : ObservableObject
    {
        private int _maLoaiDocGia;
        public int MaLoaiDocGia
        {
            get => _maLoaiDocGia;
            set
            {
                _maLoaiDocGia = value;
                OnPropertyChanged();
            }
        }

        private string _tenLoaiDocGia = string.Empty;
        public string TenLoaiDocGia
        {
            get => _tenLoaiDocGia;
            set
            {
                _tenLoaiDocGia = value;
                OnPropertyChanged();
            }
        }
    }
}
