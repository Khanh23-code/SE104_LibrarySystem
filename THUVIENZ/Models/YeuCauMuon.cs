using System;
using THUVIENZ.Core;

namespace THUVIENZ.Models
{
    public class YeuCauMuon : ObservableObject
    {
        public int MaYeuCau { get; set; }
        public int MaDocGia { get; set; }
        public int MaSach { get; set; }
        public DateTime NgayYeuCau { get; set; } = DateTime.Now;
        public string TrangThai { get; set; } = "Pending"; // Pending, Notified, Cancelled

        public virtual DocGia? DocGia { get; set; }
        public virtual Sach? Sach { get; set; }
    }
}
