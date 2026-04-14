namespace THUVIENZ.Models
{
    /// <summary>
    /// Data Transfer Object cho báo cáo tần suất mượn sách.
    /// </summary>
    public class BookStatDTO
    {
        public int MaSach { get; set; }
        public string? TenSach { get; set; }
        public int BorrowCount { get; set; }
        public int Rank { get; set; }
    }
}
