using THUVIENZ.Core;

namespace THUVIENZ.Models
{
    public class TheLoaiSach : ObservableObject
    {
        private int _maTheLoai;
        public int MaTheLoai
        {
            get => _maTheLoai;
            set
            {
                _maTheLoai = value;
                OnPropertyChanged();
            }
        }

        private string _tenTheLoai = string.Empty;
        public string TenTheLoai
        {
            get => _tenTheLoai;
            set
            {
                _tenTheLoai = value;
                OnPropertyChanged();
            }
        }
    }
}
