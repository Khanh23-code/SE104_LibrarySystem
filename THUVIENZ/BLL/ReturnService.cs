using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using THUVIENZ.DAL;

namespace THUVIENZ.BLL
{
    /// <summary>
    /// Service xử lý nghiệp vụ trả sách và tính toán tiền phạt quá hạn.
    /// </summary>
    public class ReturnService
    {
        private readonly PhieuTraRepository _phieuTraRepository;
        
        public ReturnService()
        {
            _phieuTraRepository = new PhieuTraRepository();
        }

        /// <summary>
        /// Lấy đơn giá tiền phạt theo ngày từ bảng THAMSO.
        /// </summary>
        public decimal GetLateFeeRate()
        {
            using (SqlConnection connection = DataProvider.Instance.GetConnection())
            {
                string query = "SELECT GiaTri FROM THAMSO WHERE TenThamSo = 'TienPhatMoiNgay'";
                SqlCommand cmd = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToDecimal(result) : 1000; // Mặc định 1000đ/ngày nếu không tìm thấy
                }
                catch { return 1000; }
            }
        }

        /// <summary>
        /// Lấy số ngày mượn tối đa cho phép từ bảng THAMSO.
        /// </summary>
        public int GetMaxBorrowDays()
        {
            using (SqlConnection connection = DataProvider.Instance.GetConnection())
            {
                string query = "SELECT GiaTri FROM THAMSO WHERE TenThamSo = 'SoNgayMuonToiDa'";
                SqlCommand cmd = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? (int)Convert.ToDouble(result) : 7; // Mặc định 7 ngày nếu không tìm thấy
                }
                catch { return 7; }
            }
        }

        /// <summary>
        /// Xử lý trả một cuốn sách: Tính ngày trễ, tính tiền phạt và gọi giao dịch DAL.
        /// </summary>
        public (bool Success, string Message, decimal FineAmount) ReturnBook(int maPhieuMuon, int maSach, int maDocGia, DateTime ngayMuon)
        {
            // 1. Tính toán hạn mẫu và số ngày trễ
            int maxDays = GetMaxBorrowDays();
            DateTime hanTra = ngayMuon.AddDays(maxDays);
            
            // Tính số ngày trễ (chỉ tính phần Ngày)
            TimeSpan duration = DateTime.Now.Date - hanTra.Date;
            int soNgayTre = duration.Days;
            if (soNgayTre < 0) soNgayTre = 0;

            // 2. Tính tiền phạt
            decimal rate = GetLateFeeRate();
            decimal tienPhat = soNgayTre * rate;

            // 3. Thực hiện giao dịch tại DAL
            bool result = _phieuTraRepository.ProcessReturnTransaction(maPhieuMuon, maSach, maDocGia, soNgayTre, tienPhat);

            if (result)
            {
                string msg = soNgayTre > 0 
                    ? $"Trả sách thành công. Quá hạn {soNgayTre} ngày. Tiền phạt phát sinh: {tienPhat:N0} VNĐ." 
                    : "Trả sách thành công. Sách trả đúng hạn, không phát sinh tiền phạt.";
                return (true, msg, tienPhat);
            }
            
            return (false, "Đã xảy ra lỗi hệ thống trong quá trình xử lý trả sách.", 0);
        }
    }
}
