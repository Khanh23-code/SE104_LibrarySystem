using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace THUVIENZ.DAL
{
    /// <summary>
    /// Repository quản lý phiếu mượn và các giao dịch mượn sách.
    /// Sử dụng SQL Transaction để đảm bảo tính toàn vẹn dữ liệu.
    /// </summary>
    public class PhieuMuonRepository
    {
        public bool CreateBorrowTransaction(int maDocGia, List<int> danhSachMaSach)
        {
            using (SqlConnection connection = DataProvider.Instance.GetConnection())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Tạo phiếu mượn mới (PHIEUMUON)
                        string queryPM = "INSERT INTO PHIEUMUON (MaDocGia, NgayMuon) OUTPUT INSERTED.MaPhieuMuon VALUES (@maDocGia, GETDATE())";
                        SqlCommand cmdPM = new SqlCommand(queryPM, connection, transaction);
                        cmdPM.Parameters.AddWithValue("@maDocGia", maDocGia);
                        int maPhieuMuon = (int)cmdPM.ExecuteScalar();

                        // 2. Thêm chi tiết phiếu mượn và cập nhật trạng thái sách
                        foreach (int maSach in danhSachMaSach)
                        {
                            // Thêm vào CHITIETPHIEUMUON
                            string queryCT = "INSERT INTO CHITIETPHIEUMUON (MaPhieuMuon, MaSach, TrangThai) VALUES (@maPM, @maSach, N'Đang mượn')";
                            SqlCommand cmdCT = new SqlCommand(queryCT, connection, transaction);
                            cmdCT.Parameters.AddWithValue("@maPM", maPhieuMuon);
                            cmdCT.Parameters.AddWithValue("@maSach", maSach);
                            cmdCT.ExecuteNonQuery();

                            // Cập nhật trạng thái sách trong bảng SACH
                            string querySach = "UPDATE SACH SET TinhTrang = N'Đang mượn' WHERE MaSach = @maSach";
                            SqlCommand cmdSach = new SqlCommand(querySach, connection, transaction);
                            cmdSach.Parameters.AddWithValue("@maSach", maSach);
                            cmdSach.ExecuteNonQuery();
                        }

                        // Hoàn tất giao dịch
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Hoàn tác nếu có lỗi xảy ra
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine("Lỗi giao dịch mượn sách: " + ex.Message);
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Lấy danh sách các bản ghi mượn sách đang hoạt động (chưa trả) của một độc giả.
        /// Trả về object chứa thông tin cần thiết để hiển thị và tính hạn trả.
        /// </summary>
        public System.Collections.Generic.List<dynamic> GetActiveBorrowings(int maDocGia)
        {
            var results = new System.Collections.Generic.List<dynamic>();
            using (SqlConnection connection = DataProvider.Instance.GetConnection())
            {
                string query = @"
                    SELECT PM.MaPhieuMuon, PM.NgayMuon, SA.MaSach, SA.TenSach
                    FROM PHIEUMUON PM
                    JOIN CHITIETPHIEUMUON CT ON PM.MaPhieuMuon = CT.MaPhieuMuon
                    JOIN SACH SA ON CT.MaSach = SA.MaSach
                    WHERE PM.MaDocGia = @maDG AND CT.TrangThai = N'Đang mượn'";
                
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@maDG", maDocGia);

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new {
                                MaPhieuMuon = (int)reader["MaPhieuMuon"],
                                NgayMuon = (DateTime)reader["NgayMuon"],
                                MaSach = (int)reader["MaSach"],
                                TenSach = reader["TenSach"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi GetActiveBorrowings: " + ex.Message);
                }
            }
            return results;
        }
    }
}
