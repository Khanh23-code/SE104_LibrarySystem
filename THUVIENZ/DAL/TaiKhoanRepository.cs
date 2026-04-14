using Microsoft.Data.SqlClient;
using System;
using THUVIENZ.Models;

namespace THUVIENZ.DAL
{
    /// <summary>
    /// Repositoy xử lý các truy vấn liên quan đến bảng TAIKHOAN.
    /// Tuân thủ quy tắc 3-Tier: DAL chỉ chứa logic truy vấn DB.
    /// </summary>
    public class TaiKhoanRepository
    {
        public TaiKhoan? GetAccount(string username, string password)
        {
            using (SqlConnection connection = DataProvider.Instance.GetConnection())
            {
                // Sử dụng Parameterized Query để tránh SQL Injection
                string query = "SELECT * FROM TAIKHOAN WHERE TenDangNhap = @user AND MatKhau = @pass";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@user", username);
                command.Parameters.AddWithValue("@pass", password);

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new TaiKhoan
                            {
                                TenDangNhap = reader["TenDangNhap"]?.ToString() ?? string.Empty,
                                MatKhau = reader["MatKhau"]?.ToString() ?? string.Empty,
                                Quyen = reader["Quyen"]?.ToString() ?? string.Empty
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi để debug (tương tự tinh thần SLF4J cho Java)
                    System.Diagnostics.Debug.WriteLine("Lỗi truy vấn GetAccount: " + ex.Message);
                    throw; // Ném ngoại lệ để BLL hoặc ViewModel xử lý hoặc thông báo người dùng
                }
            }
        /// <summary>
        /// Lấy thông tin tài khoản dựa trên tên đăng nhập (Dùng cho quy trình xác thực BCrypt).
        /// </summary>
        public TaiKhoan? GetAccountByUsername(string username)
        {
            using (SqlConnection connection = DataProvider.Instance.GetConnection())
            {
                string query = "SELECT * FROM TAIKHOAN WHERE TenDangNhap = @user";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@user", username);

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new TaiKhoan
                            {
                                TenDangNhap = reader["TenDangNhap"]?.ToString() ?? string.Empty,
                                MatKhau = reader["MatKhau"]?.ToString() ?? string.Empty,
                                Quyen = reader["Quyen"]?.ToString() ?? string.Empty,
                                TrangThai = reader["TrangThai"]?.ToString() ?? "Pending"
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi GetAccountByUsername: " + ex.Message);
                }
            }
            return null;
        }
    }
}
