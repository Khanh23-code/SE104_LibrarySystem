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
        public byte[]? HinhAnh { get; set; }

        public System.Windows.Media.ImageSource? ImageSource
        {
            get
            {
                if (HinhAnh == null || HinhAnh.Length == 0) return null;
                try
                {
                    var image = new System.Windows.Media.Imaging.BitmapImage();
                    using (var mem = new System.IO.MemoryStream(HinhAnh))
                    {
                        mem.Position = 0;
                        image.BeginInit();
                        image.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat;
                        image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        image.UriSource = null;
                        image.StreamSource = mem;
                        image.EndInit();
                    }
                    image.Freeze();
                    return image;
                }
                catch
                {
                    return null;
                }
            }
        }

        // UI state
        public bool IsBorrowEnabled { get; set; } = true;
        public string BorrowButtonText { get; set; } = "Mượn ngay";
    }
}
