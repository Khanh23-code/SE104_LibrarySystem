using System;

namespace THUVIENZ.ViewModels
{
    // Lightweight DTO for favorite list UI to include availability state
    public class FavoriteBookItem
    {
        public int MaSach { get; set; }
        public string TenSach { get; set; } = string.Empty;
        public string? TacGia { get; set; }
        public decimal? TriGia { get; set; }
        public string? HinhAnh { get; set; }

        // UI state
        public bool IsBorrowEnabled { get; set; } = true;
        public string BorrowButtonText { get; set; } = "Mượn ngay";
    }
}
