using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using THUVIENZ.Models;

namespace THUVIENZ.DAL
{
    /// <summary>
    /// Repository tổng hợp dữ liệu cho các báo cáo thống kê của Admin.
    /// </summary>
    public class ReportRepository
    {
        /// <summary>
        /// Thống kê tần suất mượn của các đầu sách trong một khoảng thời gian.
        /// </summary>
        public List<BookStatDTO> GetBookBorrowingStatistics(DateTime from, DateTime to)
        {
            List<BookStatDTO> stats = new List<BookStatDTO>();
            using (SqlConnection connection = DataProvider.Instance.GetConnection())
            {
                // Query kết nối 3 bảng để đếm số lần mượn theo đầu sách
                string query = @"
                    SELECT S.MaSach, S.TenSach, COUNT(CT.MaSach) as BorrowCount
                    FROM SACH S
                    LEFT JOIN CHITIETPHIEUMUON CT ON S.MaSach = CT.MaSach
                    LEFT JOIN PHIEUMUON P ON CT.MaPhieuMuon = P.MaPhieuMuon
                    WHERE P.NgayMuon >= @from AND P.NgayMuon <= @to
                    GROUP BY S.MaSach, S.TenSach
                    ORDER BY BorrowCount DESC";
                
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@from", from);
                command.Parameters.AddWithValue("@to", to);

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int rank = 1;
                        while (reader.Read())
                        {
                            stats.Add(new BookStatDTO
                            {
                                MaSach = (int)reader["MaSach"],
                                TenSach = reader["TenSach"]?.ToString() ?? "N/A",
                                BorrowCount = (int)reader["BorrowCount"],
                                Rank = rank++
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi GetBookBorrowingStatistics: " + ex.Message);
                }
            }
            return stats;
        }

        /// <summary>
        /// Báo cáo tình trạng độc giả nợ phí hoặc có sách quá hạn chưa trả.
        /// </summary>
        public List<ReaderStatDTO> GetReaderOverdueStatistics()
        {
            List<ReaderStatDTO> stats = new List<ReaderStatDTO>();
            using (SqlConnection connection = DataProvider.Instance.GetConnection())
            {
                // 1. Lấy tham số số ngày mượn tối đa từ hệ thống
                int maxDays = 7; 
                try {
                    connection.Open();
                    SqlCommand paramCmd = new SqlCommand("SELECT GiaTri FROM THAMSO WHERE TenThamSo = 'SoNgayMuonToiDa'", connection);
                    object res = paramCmd.ExecuteScalar();
                    if (res != null) maxDays = (int)Convert.ToDouble(res);
                } catch { /* Dùng mặc định nếu lỗi */ }
                finally { if (connection.State == System.Data.ConnectionState.Open) connection.Close(); }

                // 2. Query chính: Lấy độc giả có nợ hoặc có sách quá hạn (tính theo maxDays)
                string mainQuery = @"
                    SELECT D.MaDocGia, D.HoTen, ISNULL(D.TongNo, 0) as TongNo,
                           COUNT(CASE WHEN DATEADD(day, @maxDays, P.NgayMuon) < GETDATE() AND CT.TrangThai = N'Đang mượn' THEN 1 END) as OverdueCount
                    FROM DOCGIA D
                    LEFT JOIN PHIEUMUON P ON D.MaDocGia = P.MaDocGia
                    LEFT JOIN CHITIETPHIEUMUON CT ON P.MaPhieuMuon = CT.MaPhieuMuon
                    WHERE D.TongNo > 0 
                       OR (DATEADD(day, @maxDays, P.NgayMuon) < GETDATE() AND CT.TrangThai = N'Đang mượn')
                    GROUP BY D.MaDocGia, D.HoTen, D.TongNo";

                SqlCommand command = new SqlCommand(mainQuery, connection);
                command.Parameters.AddWithValue("@maxDays", maxDays);

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stats.Add(new ReaderStatDTO
                            {
                                MaDocGia = (int)reader["MaDocGia"],
                                HoTen = reader["HoTen"]?.ToString() ?? "Ẩn danh",
                                TongNo = Convert.ToDecimal(reader["TongNo"]),
                                OverdueCount = (int)reader["OverdueCount"]
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi GetReaderOverdueStatistics: " + ex.Message);
                }
            }
            return stats;
        }
    }
}
