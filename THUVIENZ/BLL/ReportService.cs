using System;
using System.Collections.Generic;
using System.Linq;
using THUVIENZ.DAL;
using THUVIENZ.Models;

namespace THUVIENZ.BLL
{
    /// <summary>
    /// Service xử lý logic tổng hợp và định dạng báo cáo.
    /// </summary>
    public class ReportService
    {
        private readonly ReportRepository _reportRepository;

        public ReportService()
        {
            _reportRepository = new ReportRepository();
        }

        /// <summary>
        /// Lấy danh sách các đầu sách được mượn nhiều nhất trong khoảng thời gian.
        /// </summary>
        public List<BookStatDTO> GetTopBorrowedBooks(DateTime from, DateTime to, int topCount = 10)
        {
            // Đảm bảo logic ngày giờ (từ 00:00:00 của FromDate đến 23:59:59 của ToDate)
            DateTime startDate = from.Date;
            DateTime endDate = to.Date.AddDays(1).AddTicks(-1);

            var allStats = _reportRepository.GetBookBorrowingStatistics(startDate, endDate);
            
            // Trả về Top X kết quả
            return allStats.Take(topCount).ToList();
        }

        /// <summary>
        /// Lấy thống kê về tình hình công nợ và quá hạn của độc giả.
        /// </summary>
        public List<ReaderStatDTO> GetReaderOverdueReports()
        {
            return _reportRepository.GetReaderOverdueStatistics();
        }
    }
}
