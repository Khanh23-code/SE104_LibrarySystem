using THUVIENZ.Core;

namespace THUVIENZ.Models
{
    public class ThamSo : ObservableObject
    {
        private string _tenThamSo = string.Empty;
        public string TenThamSo
        {
            get => _tenThamSo;
            set
            {
                _tenThamSo = value;
                OnPropertyChanged();
            }
        }

        private double _giaTri;
        public double GiaTri
        {
            get => _giaTri;
            set
            {
                _giaTri = value;
                OnPropertyChanged();
            }
        }
    }
}
