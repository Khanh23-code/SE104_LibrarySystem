using System;
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

        public AuthService()
        {
            _taiKhoanRepository = new TaiKhoanRepository();
        }

        /// <summary>
        /// Xử lý logic đăng nhập với cơ chế kiểm tra BCrypt Hash và Plaintext.
        /// </summary>
        /// <returns>Quyền của tài khoản (Admin/Reader) nếu thành công và Active, ngược lại trả về null.</returns>
        public string? Login(string username, string password)
        {
            // 1. Kiểm tra validation cơ bản
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            try
            {
                // 2. Lấy thông tin tài khoản từ DB theo Username (bao gồm cả cột mật khẩu đã lưu)
                TaiKhoan? account = _taiKhoanRepository.GetAccountByUsername(username);

                if (account == null) return null;

                // 3. Kiểm tra trạng thái tài khoản (FT-2)
                if (account.TrangThai != "Active")
                {
                    // Tài khoản chưa được duyệt hoặc bị khóa
                    return "PENDING_OR_LOCKED";
                }

                bool isPasswordValid = false;

                // 4. XÁC THỰC BẢO MẬT (BCrypt)
                try
                {
                    // Thử kiểm tra theo chuẩn BCrypt Hash
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(password, account.MatKhau);
                }
                catch (Exception)
                {
                    // Nếu dữ liệu trong DB không phải dạng Hash (BCrypt sẽ quăng lỗi), 
                    // ta sử dụng cơ chế FALLBACK: So sánh văn bản thuần túy (Plaintext).
                    if (password == account.MatKhau)
                    {
                        isPasswordValid = true;
                        // Gợi ý: Tại đây có thể thực hiện Re-hash mật khẩu và cập nhật vào DB để nâng cấp bảo mật từ từ.
                    }
                }

                // 5. Trả về quyền hạn nếu hợp lệ
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
