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

        private string? _anhDaiDien;
        /// <summary>
        /// Ảnh đại diện của độc giả.
        /// </summary>
        public string? AnhDaiDien
        {
            get => _anhDaiDien;
            set
            {
                _anhDaiDien = value;
                OnPropertyChanged();
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
