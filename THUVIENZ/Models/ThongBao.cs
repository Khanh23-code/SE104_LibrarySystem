using System;
using THUVIENZ.Core;

namespace THUVIENZ.Models
{
    public class ThongBao : ObservableObject
    {
        public int MaThongBao { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string LoaiThongBao { get; set; } = "Info"; // Success, Failure, Warning, Info
        public DateTime NgayThongBao { get; set; } = DateTime.Now;
        public bool DaDoc { get; set; } = false;

        public virtual TaiKhoan? TaiKhoan { get; set; }
    }
}
