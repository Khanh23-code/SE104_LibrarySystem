using System;
using System.Threading.Tasks;
using THUVIENZ.DAL;
using THUVIENZ.Models;
using BCrypt.Net;

namespace THUVIENZ.BLL
{
    /// <summary>
    /// Service xử lý các nghiệp vụ liên quan đến xác thực và phân quyền.
    /// Đã nâng cấp bảo mật bằng BCrypt kèm cơ chế tương thích ngược (Fallback).
    /// </summary>
    public class AuthService
    {
        private readonly TaiKhoanRepository _taiKhoanRepository;

        public AuthService() : this(new TaiKhoanRepository())
        {
        }

        /// <summary>
        /// Lấy thông tin hồ sơ (DocGia) liên kết với tài khoản đang ở trạng thái Pending.
        /// Trả về null nếu không tồn tại.
        /// </summary>
        public async Task<DocGia?> GetPendingProfileAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;

            var account = await _taiKhoanRepository.GetAccountByUsernameAsync(username);
            if (account == null) return null;
            if (account.TrangThai != "Pending") return null;

            // Tải DocGia nếu có
            return account.DocGia;
        }

        /// <summary>
        /// Đăng ký tài khoản mới. Tài khoản mới sẽ có trạng thái "Pending" để chờ admin duyệt.
        /// Chú ý: chữ ký phương thức giữ các tham số email/phone/address là nullable để tương thích.
        /// Trả về true nếu tạo thành công, false nếu đã tồn tại hoặc lỗi.
        /// </summary>
        public async Task<bool> RegisterAsync(string username, string password, string role = "Reader", string? fullName = null, string? gender = null, string? email = null, string? phone = null, string? address = null)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return false;

            var existing = await _taiKhoanRepository.GetByIdAsync(username);
            if (existing != null) return false; // đã tồn tại

            var hashed = BCrypt.Net.BCrypt.HashPassword(password);

            var account = new Models.TaiKhoan
            {
                TenDangNhap = username,
                MatKhau = hashed,
                Quyen = role,
                TrangThai = "Pending"
            };

            // Nếu user đã cung cấp thông tin hồ sơ, tạo DocGia liên kết để admin có thể xem khi xét duyệt
            if (!string.IsNullOrWhiteSpace(fullName) || !string.IsNullOrWhiteSpace(gender) || !string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(phone) || !string.IsNullOrWhiteSpace(address))
            {
                var reader = new DocGia
                {
                    TenDangNhap = username,
                    HoTen = fullName ?? username,
                    GioiTinh = gender ?? "Khác",
                    Email = email,
                    SoDienThoai = phone,
                    DiaChi = address,
                    NgayLapThe = System.DateTime.Now
                };

                account.DocGia = reader;
            }

            await _taiKhoanRepository.AddAsync(account);
            await _taiKhoanRepository.SaveChangesAsync();
            return true;
        }

        public AuthService(TaiKhoanRepository repository)
        {
            _taiKhoanRepository = repository;
        }

        /// <summary>
        /// Xử lý logic đăng nhập với cơ chế kiểm tra BCrypt Hash và Plaintext.
        /// </summary>
        public async Task<string?> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            try
            {
                TaiKhoan? account = await _taiKhoanRepository.GetAccountByUsernameAsync(username);

                if (account == null) return null;

                if (account.TrangThai != "Active")
                {
                    return "PENDING_OR_LOCKED";
                }

                bool isPasswordValid = false;

                try
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(password, account.MatKhau);
                }
                catch (Exception)
                {
                    if (password == account.MatKhau)
                    {
                        isPasswordValid = true;
                    }
                }

                if (isPasswordValid)
                {
                    return account.Quyen;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi Auth: " + ex.Message);
                throw;
            }

            return null;
        }
    }
}
