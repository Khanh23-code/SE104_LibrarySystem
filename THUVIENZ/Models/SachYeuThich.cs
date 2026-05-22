using System;
using THUVIENZ.Core;

namespace THUVIENZ.Models
{
    public class SachYeuThich : ObservableObject
    {
        public int MaDocGia { get; set; }
        public int MaSach { get; set; }
        public DateTime NgayThem { get; set; } = DateTime.Now;

        public virtual DocGia? DocGia { get; set; }
        public virtual Sach? Sach { get; set; }
    }
}
