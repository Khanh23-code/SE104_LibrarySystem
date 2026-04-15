using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using THUVIENZ.Models;

namespace THUVIENZ.DAL
{
    /// <summary>
    /// Repository xử lý dữ liệu liên quan đến Độc giả và thông tin mượn trả của họ.
    /// </summary>
    public class DocGiaRepository
    {
        /// <summary>
        /// Lấy thông tin chi tiết của Độc giả dựa trên tên đăng nhập.
        /// </summary>
        public DocGia? GetReaderProfile(string username)
        {
            using (SqlConnection connection = DataProvider.Instance.GetConnection())
            {
                string query = "SELECT * FROM DOCGIA WHERE TenDangNhap = @username";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@username", username);

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new DocGia
                            {
                                MaDocGia = Convert.ToInt32(reader["MaDocGia"]),
                                HoTen = reader["HoTen"]?.ToString() ?? string.Empty,
                                MaLoaiDocGia = reader["MaLoaiDocGia"] != DBNull.Value ? (int?)reader["MaLoaiDocGia"] : null,
                                NgaySinh = reader["NgaySinh"] != DBNull.Value ? (DateTime?)reader["NgaySinh"] : null,
                                DiaChi = reader["DiaChi"]?.ToString(),
                                Email = reader["Email"]?.ToString(),
                                NgayLapThe = reader["NgayLapThe"] != DBNull.Value ? (DateTime?)reader["NgayLapThe"] : null,
                                TongNo = reader["TongNo"] != DBNull.Value ? (decimal?)reader["TongNo"] : null,
                                TenDangNhap = reader["TenDangNhap"]?.ToString()
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi GetReaderProfile: " + ex.Message);
                    throw;
                }
            }
            return null;
        }

        /// <summary>
        /// Lấy danh sách các cuốn sách mà Độc giả đang mượn (Trạng thái: Đang mượn).
        /// </summary>
        public List<Sach> GetBorrowedBooks(int maDocGia)
        {
            List<Sach> borrowedBooks = new List<Sach>();
            using (SqlConnection connection = DataProvider.Instance.GetConnection())
            {
                // Truy vấn JOIN để lấy thông tin sách từ các phiếu mượn chưa trả
                string query = @"
                    SELECT S.* 
                    FROM SACH S
                    JOIN CHITIETPHIEUMUON CT ON S.MaSach = CT.MaSach
                    JOIN PHIEUMUON P ON CT.MaPhieuMuon = P.MaPhieuMuon
                    WHERE P.MaDocGia = @maDocGia AND CT.TrangThai = N'Đang mượn'";
                
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@maDocGia", maDocGia);

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            borrowedBooks.Add(new Sach
                            {
                                MaSach = Convert.ToInt32(reader["MaSach"]),
                                TenSach = reader["TenSach"]?.ToString() ?? string.Empty,
                                MaTheLoai = reader["MaTheLoai"] != DBNull.Value ? (int?)reader["MaTheLoai"] : null,
                                TacGia = reader["TacGia"]?.ToString(),
                                NamXuatBan = reader["NamXuatBan"] != DBNull.Value ? (int?)reader["NamXuatBan"] : null,
                                NhaXuatBan = reader["NhaXuatBan"]?.ToString(),
                                NgayNhap = reader["NgayNhap"] != DBNull.Value ? (DateTime?)reader["NgayNhap"] : null,
                                TriGia = reader["TriGia"] != DBNull.Value ? (decimal?)reader["TriGia"] : null,
                                TinhTrang = reader["TinhTrang"]?.ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi GetBorrowedBooks: " + ex.Message);
                    throw;
                }
            }
            return borrowedBooks;
        }
        /// <summary>
        /// Truy vấn số tiền nợ hiện tại của Độc giả.
        /// </summary>
        public decimal GetReaderDebt(int maDocGia)
        {
            using (SqlConnection connection = DataProvider.Instance.GetConnection())
            {
                string query = "SELECT ISNULL(TongNo, 0) FROM DOCGIA WHERE MaDocGia = @maDG";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@maDG", maDocGia);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    return result != null ? Convert.ToDecimal(result) : 0;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi GetReaderDebt: " + ex.Message);
                    return 0;
                }
            }
        }
    }
}
