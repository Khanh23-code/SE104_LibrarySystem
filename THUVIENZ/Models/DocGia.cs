using System;
using System.Collections.Generic;
using THUVIENZ.Core;

namespace THUVIENZ.Models
{
    /// <summary>
    /// Thực thể đại diện cho bảng DOCGIA trong Database.
    /// Đã bổ sung các trường chuẩn UI: Giới tính, Số điện thoại.
    /// Áp dụng Strict Null Safety tuyệt đối và chú thích Tiếng Việt.
    /// </summary>
    public class DocGia : ObservableObject
    {
        private int _maDocGia;
        /// <summary>
        /// Mã độc giả (Khóa chính tự tăng).
        /// </summary>
        public int MaDocGia
        {
            get => _maDocGia;
            set
            {
                _maDocGia = value;
                OnPropertyChanged();
            }
        }

        private string? _tenDangNhap;
        /// <summary>
        /// Tên đăng nhập ánh xạ 1-1 với tài khoản hệ thống (Khóa ngoại UNIQUE).
        /// </summary>
        public string? TenDangNhap
        {
            get => _tenDangNhap;
            set
            {
                _tenDangNhap = value;
                OnPropertyChanged();
            }
        }

        private string _hoTen = string.Empty;
        /// <summary>
        /// Họ và tên đầy đủ của độc giả.
        /// </summary>
        public string HoTen
        {
            get => _hoTen;
            set
            {
                _hoTen = value;
                OnPropertyChanged();
            }
        }

        private int? _maLoaiDocGia;
        /// <summary>
        /// Mã loại độc giả (Khóa ngoại).
        /// </summary>
        public int? MaLoaiDocGia
        {
            get => _maLoaiDocGia;
            set
            {
                _maLoaiDocGia = value;
                OnPropertyChanged();
            }
        }

        private string? _gioiTinh;
        /// <summary>
        /// Giới tính (Nam, Nữ, Khác).
        /// </summary>
        public string? GioiTinh
        {
            get => _gioiTinh;
            set
            {
                _gioiTinh = value;
                OnPropertyChanged();
            }
        }

        private string? _soDienThoai;
        /// <summary>
        /// Số điện thoại liên lạc.
        /// </summary>
        public string? SoDienThoai
        {
            get => _soDienThoai;
            set
            {
                _soDienThoai = value;
                OnPropertyChanged();
            }
        }

        private string? _email;
        /// <summary>
        /// Địa chỉ Email liên lạc.
        /// </summary>
        public string? Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        private string? _diaChi;
        /// <summary>
        /// Địa chỉ thường trú.
        /// </summary>
        public string? DiaChi
        {
            get => _diaChi;
            set
            {
                _diaChi = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _ngaySinh;
        /// <summary>
        /// Ngày tháng năm sinh.
        /// </summary>
        public DateTime? NgaySinh
        {
            get => _ngaySinh;
            set
            {
                _ngaySinh = value;
                OnPropertyChanged();
            }
        }

        private DateTime _ngayLapThe = DateTime.Now;
        /// <summary>
        /// Ngày cấp thẻ thư viện.
        /// </summary>
        public DateTime NgayLapThe
        {
            get => _ngayLapThe;
            set
            {
                _ngayLapThe = value;
                OnPropertyChanged();
            }
        }

        private decimal _tongNo;
        /// <summary>
        /// Tổng nợ tiền phạt hiện tại.
        /// </summary>
        public decimal TongNo
        {
            get => _tongNo;
            set
            {
                _tongNo = value;
                OnPropertyChanged();
            }
        }

        private byte[]? _anhDaiDien;
        /// <summary>
        /// Ảnh đại diện của độc giả lưu dạng nhị phân.
        /// </summary>
        public byte[]? AnhDaiDien
        {
            get => _anhDaiDien;
            set
            {
                _anhDaiDien = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AvatarSource));
            }
        }

        /// <summary>
        /// Đối tượng ImageSource phục vụ hiển thị ảnh đại diện trên UI WPF.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public System.Windows.Media.ImageSource? AvatarSource
        {
            get
            {
                if (AnhDaiDien == null || AnhDaiDien.Length == 0) return null;
                try
                {
                    var image = new System.Windows.Media.Imaging.BitmapImage();
                    using (var mem = new System.IO.MemoryStream(AnhDaiDien))
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

        /// <summary>
        /// Đối tượng Loại Độc giả liên kết (Quan hệ N-1).
        /// </summary>
        public virtual LoaiDocGia? LoaiDocGia { get; set; }

        /// <summary>
        /// Đối tượng Tài khoản liên kết (Quan hệ 1-1).
        /// </summary>
        public virtual TaiKhoan? TaiKhoan { get; set; }

        /// <summary>
        /// Danh sách các phiếu mượn của độc giả này (Quan hệ 1-N).
        /// </summary>
        public virtual ICollection<PhieuMuon> PhieuMuons { get; set; } = new List<PhieuMuon>();

        /// <summary>
        /// Danh sách các phiếu thu tiền phạt của độc giả này (Quan hệ 1-N).
        /// </summary>
        public virtual ICollection<PhieuThuTienPhat> PhieuThuTienPhats { get; set; } = new List<PhieuThuTienPhat>();
    }
}
