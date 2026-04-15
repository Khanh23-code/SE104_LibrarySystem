namespace THUVIENZ.Models
{
    /// <summary>
    /// Data Transfer Object cho báo cáo độc giả trễ hạn và nợ.
    /// </summary>
    public class ReaderStatDTO
    {
        public int MaDocGia { get; set; }
        public string? HoTen { get; set; }
        public decimal TongNo { get; set; }
        public int OverdueCount { get; set; }
    }
}
