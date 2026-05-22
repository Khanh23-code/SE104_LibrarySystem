using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using THUVIENZ.Core;

namespace THUVIENZ.Models
{
    /// <summary>
    /// Thực thể đại diện cho bảng SACH (Đầu Sách gốc) trong Database.
    /// Không còn chứa trực tiếp các cột số lượng hay tình trạng vật lý ở DB.
    /// Áp dụng Strict Null Safety tuyệt đối và chú thích Tiếng Việt.
    /// </summary>
    public class Sach : ObservableObject
    {
        private int _maSach;
        /// <summary>
        /// Mã đầu sách (Khóa chính tự tăng).
        /// </summary>
        public int MaSach
        {
            get => _maSach;
            set
            {
                _maSach = value;
                OnPropertyChanged();
            }
        }

        private string? _maISBN;
        /// <summary>
        /// Mã định danh ISBN/Mã ID Sách hiển thị trên giao diện UI.
        /// </summary>
        public string? MaISBN
        {
            get => _maISBN;
            set
            {
                _maISBN = value;
                OnPropertyChanged();
            }
        }

        private string _tenSach = string.Empty;
        /// <summary>
        /// Tên đầu sách.
        /// </summary>
        public string TenSach
        {
            get => _tenSach;
            set
            {
                _tenSach = value;
                OnPropertyChanged();
            }
        }

        private int? _maTheLoai;
        /// <summary>
        /// Mã thể loại sách (Khóa ngoại).
        /// </summary>
        public int? MaTheLoai
        {
            get => _maTheLoai;
            set
            {
                _maTheLoai = value;
                OnPropertyChanged();
            }
        }

        private string? _tacGia;
        /// <summary>
        /// Tên tác giả.
        /// </summary>
        public string? TacGia
        {
            get => _tacGia;
            set
            {
                _tacGia = value;
                OnPropertyChanged();
            }
        }

        private string? _nhaXuatBan;
        /// <summary>
        /// Tên nhà xuất bản.
        /// </summary>
        public string? NhaXuatBan
        {
            get => _nhaXuatBan;
            set
            {
                _nhaXuatBan = value;
                OnPropertyChanged();
            }
        }

        private int? _namXuatBan;
        /// <summary>
        /// Năm xuất bản.
        /// </summary>
        public int? NamXuatBan
        {
            get => _namXuatBan;
            set
            {
                _namXuatBan = value;
                OnPropertyChanged();
            }
        }

        private string _ngonNgu = "Tiếng Việt";
        /// <summary>
        /// Ngôn ngữ của sách (Mặc định Tiếng Việt).
        /// </summary>
        public string NgonNgu
        {
            get => _ngonNgu;
            set
            {
                _ngonNgu = value;
                OnPropertyChanged();
            }
        }

        private decimal? _triGia;
        /// <summary>
        /// Trị giá cuốn sách.
        /// </summary>
        public decimal? TriGia
        {
            get => _triGia;
            set
            {
                _triGia = value;
                OnPropertyChanged();
            }
        }

        private string? _moTa;
        /// <summary>
        /// Mô tả nội dung tóm tắt của sách.
        /// </summary>
        public string? MoTa
        {
            get => _moTa;
            set
            {
                _moTa = value;
                OnPropertyChanged();
            }
        }

        private string? _hinhAnh;
        /// <summary>
        /// Đường dẫn lưu trữ hình ảnh bìa sách local.
        /// </summary>
        public string? HinhAnh
        {
            get => _hinhAnh;
            set
            {
                _hinhAnh = value;
                OnPropertyChanged();
            }
        }

        private byte[] _rowVersion = Array.Empty<byte>();
        /// <summary>
        /// RowVersion hỗ trợ cơ chế Optimistic Concurrency Control (OCC).
        /// </summary>
        [System.ComponentModel.DataAnnotations.Timestamp]
        public byte[] RowVersion
        {
            get => _rowVersion;
            set
            {
                _rowVersion = value;
                OnPropertyChanged();
            }
        }

        // ====================================================================
        // THUỘC TÍNH KHÔNG ÁNH XẠ VÀO DB (CHỈ DÙNG CHO UI BINDING TRÊN WPF)
        // ====================================================================

        private bool _isFavorite;
        /// <summary>
        /// Trạng thái yêu thích của sách đối với người dùng hiện tại (NotMapped).
        /// </summary>
        [NotMapped]
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                _isFavorite = value;
                OnPropertyChanged();
            }
        }

        private int _soLuong = 1;
        /// <summary>
        /// Số lượng bản sao (Thuộc tính NotMapped, hỗ trợ UI binding nhập liệu).
        /// </summary>
        [NotMapped]
        public int SoLuong
        {
            get
            {
                // Nếu collection đã được nạp và có phần tử, ưu tiên đếm từ danh sách thực tế
                if (CuonSachs != null && CuonSachs.Any())
                    return CuonSachs.Count;
                return _soLuong;
            }
            set
            {
                _soLuong = value;
                OnPropertyChanged();
            }
        }

        private string? _tinhTrang;
        /// <summary>
        /// Tình trạng khả dụng (Thuộc tính NotMapped, hỗ trợ UI binding).
        /// </summary>
        [NotMapped]
        public string? TinhTrang
        {
            get
            {
                if (CuonSachs != null && CuonSachs.Any())
                {
                    bool hasAvailable = CuonSachs.Any(cs => cs.TinhTrang == "Sẵn sàng");
                    return hasAvailable ? "Còn sách" : "Hết sách";
                }
                return string.IsNullOrEmpty(_tinhTrang) ? "Còn sách" : _tinhTrang;
            }
            set
            {
                _tinhTrang = value;
                OnPropertyChanged();
            }
        }

        // ====================================================================
        // CÁC QUAN HỆ ĐIỀU HƯỚNG (NAVIGATION PROPERTIES)
        // ====================================================================

        /// <summary>
        /// Đối tượng Thể loại sách liên kết (Quan hệ N-1).
        /// </summary>
        public virtual TheLoaiSach? TheLoaiSach { get; set; }

        /// <summary>
        /// Danh sách các bản sao vật lý của đầu sách này (Quan hệ 1-N).
        /// </summary>
        public virtual ICollection<CuonSach> CuonSachs { get; set; } = new List<CuonSach>();
    }
}
