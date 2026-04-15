using Microsoft.Data.SqlClient;
using System;

namespace THUVIENZ.DAL
{
    /// <summary>
    /// Repository xử lý nghiệp vụ trả sách và tính toán tiền phạt.
    /// Sử dụng giao dịch tập trung để đảm bảo tính nhất quán dữ liệu.
    /// </summary>
    public class PhieuTraRepository
    {
        /// <summary>
        /// Thực hiện giao dịch trả sách liên phòng ban (Sách, Phiếu mượn, Tiền nợ độc giả).
        /// </summary>
        public bool ProcessReturnTransaction(int maPhieuMuon, int maSach, int maDocGia, int soNgayTre, decimal tienPhat)
        {
            using (SqlConnection connection = DataProvider.Instance.GetConnection())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Cập nhật trạng thái sách trong chi tiết phiếu mượn gốc
                        string queryCTPM = "UPDATE CHITIETPHIEUMUON SET TrangThai = N'Đã trả' WHERE MaPhieuMuon = @maPM AND MaSach = @maSach";
                        SqlCommand cmdCTPM = new SqlCommand(queryCTPM, connection, transaction);
                        cmdCTPM.Parameters.AddWithValue("@maPM", maPhieuMuon);
                        cmdCTPM.Parameters.AddWithValue("@maSach", maSach);
                        cmdCTPM.ExecuteNonQuery();

                        // 2. Cập nhật trạng thái đầu sách về 'Sẵn sàng'
                        string querySach = "UPDATE SACH SET TinhTrang = N'Sẵn sàng' WHERE MaSach = @maSach";
                        SqlCommand cmdSach = new SqlCommand(querySach, connection, transaction);
                        cmdSach.Parameters.AddWithValue("@maSach", maSach);
                        cmdSach.ExecuteNonQuery();

                        // 3. Tạo một phiếu trả (PHIEUTRA) mới cho giao dịch này
                        string queryPT = "INSERT INTO PHIEUTRA (MaDocGia, NgayTra, TienPhatKyNay) OUTPUT INSERTED.MaPhieuTra VALUES (@maDG, GETDATE(), @tienPhat)";
                        SqlCommand cmdPT = new SqlCommand(queryPT, connection, transaction);
                        cmdPT.Parameters.AddWithValue("@maDG", maDocGia);
                        cmdPT.Parameters.AddWithValue("@tienPhat", tienPhat);
                        int maPhieuTra = (int)cmdPT.ExecuteScalar();

                        // 4. Ghi nhận chi tiết phiếu trả (CHITIETPHIEUTRA)
                        string queryCTPT = "INSERT INTO CHITIETPHIEUTRA (MaPhieuTra, MaPhieuMuon, MaSach, SoNgayMuon, TienPhat) VALUES (@maPT, @maPM, @maSach, @soNgay, @tienPhat)";
                        SqlCommand cmdCTPT = new SqlCommand(queryCTPT, connection, transaction);
                        cmdCTPT.Parameters.AddWithValue("@maPT", maPhieuTra);
                        cmdCTPT.Parameters.AddWithValue("@maPM", maPhieuMuon);
                        cmdCTPT.Parameters.AddWithValue("@maSach", maSach);
                        cmdCTPT.Parameters.AddWithValue("@soNgay", soNgayTre); // Lưu số ngày bị trễ
                        cmdCTPT.Parameters.AddWithValue("@tienPhat", tienPhat);
                        cmdCTPT.ExecuteNonQuery();

                        // 5. Nếu có tiền phạt, cộng vào tổng nợ (TongNo) của Độc giả
                        if (tienPhat > 0)
                        {
                            string queryDG = "UPDATE DOCGIA SET TongNo = ISNULL(TongNo, 0) + @tienPhat WHERE MaDocGia = @maDG";
                            SqlCommand cmdDG = new SqlCommand(queryDG, connection, transaction);
                            cmdDG.Parameters.AddWithValue("@tienPhat", tienPhat);
                            cmdDG.Parameters.AddWithValue("@maDG", maDocGia);
                            cmdDG.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine("Lỗi ProcessReturnTransaction: " + ex.Message);
                        return false;
                    }
                }
            }
        }
    }
}
