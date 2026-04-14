using System.Collections.Generic;
using THUVIENZ.DAL;

namespace THUVIENZ.BLL
{
    /// <summary>
    /// Service xử lý các nghiệp vụ lưu thông sách (Mượn/Trả).
    /// </summary>
    public class CirculationService
    {
        private readonly PhieuMuonRepository _phieuMuonRepository;
        private readonly SachRepository _sachRepository;
        private readonly DocGiaRepository _docGiaRepository;

        public CirculationService()
        {
            _phieuMuonRepository = new PhieuMuonRepository();
            _sachRepository = new SachRepository();
            _docGiaRepository = new DocGiaRepository();
        }

        /// <summary>
        /// Xử lý yêu cầu mượn sách của độc giả.
        /// </summary>
        public (bool Success, string Message) BorrowBooks(int maDocGia, List<int> danhSachMaSach)
        {
            // 1. Kiểm tra đầu vào
            if (maDocGia <= 0) return (false, "Mã độc giả không hợp lệ.");
            if (danhSachMaSach == null || danhSachMaSach.Count == 0) return (false, "Danh sách sách mượn trống.");

            // 2. [QUY TẮC NGHIỆP VỤ: FT-2] Kiểm tra nợ tối đa của độc giả
            decimal currentDebt = _docGiaRepository.GetReaderDebt(maDocGia);
            decimal maxDebtAllowed = 50000; // Giá trị mặc định

            // Thử lấy giá trị cấu hình từ THAMSO
            using (var conn = DataProvider.Instance.GetConnection())
            {
                var cmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT GiaTri FROM THAMSO WHERE TenThamSo = 'TongNoToiDa'", conn);
                try {
                    conn.Open();
                    var res = cmd.ExecuteScalar();
                    if (res != null) maxDebtAllowed = Convert.ToDecimal(res);
                } catch { /* Dùng mặc định nếu lỗi */ }
            }

            if (currentDebt > maxDebtAllowed)
            {
                return (false, $"Độc giả đang nợ {currentDebt:N0} VNĐ, vượt quá giới hạn quy định ({maxDebtAllowed:N0} VNĐ). Vui lòng thanh toán trước khi mượn thêm.");
            }

            // 3. Kiểm tra trạng thái khả dụng của từng cuốn sách
            foreach (int maSach in danhSachMaSach)
            {
                // Sử dụng lại logic kiểm tra từ SachRepository
                if (_sachRepository.IsBookCurrentlyBorrowed(maSach))
                {
                    return (false, $"Sách có mã {maSach} hiện đang được mượn bởi người khác.");
                }
            }

            // 3. Thực hiện giao dịch tại tầng DAL
            bool result = _phieuMuonRepository.CreateBorrowTransaction(maDocGia, danhSachMaSach);

            if (result)
            {
                return (true, "Yêu cầu mượn sách đã được thực hiện thành công.");
            }
            else
            {
                return (false, "Đã xảy ra lỗi trong quá trình xử lý giao dịch tại hệ thống.");
            }
        }
    }
}
