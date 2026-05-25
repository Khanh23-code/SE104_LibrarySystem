using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using THUVIENZ.DAL;
using THUVIENZ.Models;

namespace THUVIENZ.BLL
{
    /// <summary>
    /// Service xử lý báo cáo và khai phá dữ liệu (Data Mining).
    /// Áp dụng Strict Null Safety và cấu trúc DB chuẩn hóa mới gộp chung.
    /// Toàn bộ chú thích viết bằng Tiếng Việt.
    /// </summary>
    public class ReportService
    {
        private readonly LmsDbContext? _context;

        public ReportService()
        {
        }

        public ReportService(LmsDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách các cặp sách thường được mượn cùng nhau kèm theo tên sách.
        /// Dùng cho hiển thị UI.
        /// </summary>
        public async Task<List<BookPairDTO>> GetFrequentBookPairsWithNamesAsync(int minSupport = 2)
        {
            using var localContext = _context == null ? new LmsDbContext() : null;
            var context = _context ?? localContext!;

            var pairs = await GetFrequentBookPairsAsync(minSupport);
            if (!pairs.Any()) return new List<BookPairDTO>();

            var bookIds = pairs.Select(p => p.Item1).Union(pairs.Select(p => p.Item2)).Distinct().ToList();
            var bookNames = await context.Sachs
                .Where(s => bookIds.Contains(s.MaSach))
                .ToDictionaryAsync(s => s.MaSach, s => s.TenSach);

            return pairs.Select(p => new BookPairDTO
            {
                Book1Name = bookNames.ContainsKey(p.Item1) ? bookNames[p.Item1] : "Sách " + p.Item1,
                Book2Name = bookNames.ContainsKey(p.Item2) ? bookNames[p.Item2] : "Sách " + p.Item2,
                SupportCount = p.Item3
            }).ToList();
        }

        /// <summary>
        /// Khai phá mẫu mượn sách bằng thuật toán Apriori đơn giản.
        /// Tìm các cặp đầu sách thường xuyên được mượn cùng nhau.
        /// </summary>
        /// <param name="minSupport">Độ hỗ trợ tối thiểu (số lần xuất hiện)</param>
        public async Task<List<Tuple<int, int, int>>> GetFrequentBookPairsAsync(int minSupport = 2)
        {
            using var localContext = _context == null ? new LmsDbContext() : null;
            var context = _context ?? localContext!;

            // 1. Lấy dữ liệu mượn sách theo từng giao dịch (Phiếu mượn)
            var items = await context.ChiTietMuonTras
                .Include(c => c.CuonSach)
                .Select(c => new { c.MaPhieuMuon, MaSach = c.CuonSach!.MaSach })
                .ToListAsync();

            var transactions = items
                .GroupBy(x => x.MaPhieuMuon)
                .Select(g => g.Select(x => x.MaSach).ToList())
                .ToList();

            var pairCounts = new Dictionary<Tuple<int, int>, int>();

            // 2. Thuật toán Apriori: Tạo ứng viên bộ 2 (2-itemsets)
            foreach (var transaction in transactions)
            {
                if (transaction.Count < 2) continue;

                var sortedItems = transaction.Distinct().OrderBy(i => i).ToList();
                for (int i = 0; i < sortedItems.Count; i++)
                {
                    for (int j = i + 1; j < sortedItems.Count; j++)
                    {
                        var pair = new Tuple<int, int>(sortedItems[i], sortedItems[j]);
                        if (pairCounts.ContainsKey(pair))
                            pairCounts[pair]++;
                        else
                            pairCounts[pair] = 1;
                    }
                }
            }

            // 3. Lọc theo độ hỗ trợ tối thiểu (Support)
            return pairCounts
                .Where(p => p.Value >= minSupport)
                .Select(p => new Tuple<int, int, int>(p.Key.Item1, p.Key.Item2, p.Value))
                .OrderByDescending(p => p.Item3)
                .ToList();
        }

        /// <summary>
        /// Lấy tóm tắt các con số KPI cho Dashboard.
        /// </summary>
        public async Task<DashboardSummaryDTO> GetDashboardSummaryAsync()
        {
            using var localContext = _context == null ? new LmsDbContext() : null;
            var context = _context ?? localContext!;

            var totalBooks = await context.Sachs.CountAsync();
            var totalReaders = await context.DocGias.CountAsync();
            
            // Sách đang mượn (NgayTraThucTe == null)
            var borrowedBooks = await context.ChiTietMuonTras
                .CountAsync(c => c.NgayTraThucTe == null);

            // Sách trễ hạn (NgayTraThucTe == null và HanTra < Now)
            var overdueBooks = await context.ChiTietMuonTras
                .CountAsync(c => c.NgayTraThucTe == null && c.HanTra < DateTime.Now);

            return new DashboardSummaryDTO
            {
                TotalBooks = totalBooks,
                TotalReaders = totalReaders,
                BorrowedBooks = borrowedBooks,
                OverdueBooks = overdueBooks
            };
        }

        /// <summary>
        /// Lấy xu hướng mượn sách và trễ hạn trong khoảng thời gian xác định.
        /// Thường dùng cho biểu đồ đường.
        /// </summary>
        public async Task<List<BorrowingTrendDTO>> GetBorrowingTrendAsync(DateTime from, DateTime to)
        {
            using var localContext = _context == null ? new LmsDbContext() : null;
            var context = _context ?? localContext!;

            var trend = new List<BorrowingTrendDTO>();
            var allData = await context.ChiTietMuonTras
                .Include(c => c.PhieuMuon)
                .Where(c => c.PhieuMuon!.NgayMuon >= from && c.PhieuMuon!.NgayMuon <= to)
                .Select(c => new { c.PhieuMuon!.NgayMuon, c.HanTra, c.NgayTraThucTe })
                .ToListAsync();

            // Nhóm theo ngày ở tầng ứng dụng vì SQL Lite/Server DATE casting phức tạp với EF Core
            var grouped = allData
                .GroupBy(c => c.NgayMuon.Date)
                .OrderBy(g => g.Key);

            foreach (var group in grouped)
            {
                trend.Add(new BorrowingTrendDTO
                {
                    Date = group.Key,
                    BorrowCount = group.Count(),
                    OverdueCount = group.Count(c => (c.NgayTraThucTe == null && c.HanTra < DateTime.Now) || 
                                                   (c.NgayTraThucTe != null && c.NgayTraThucTe > c.HanTra))
                });
            }

            return trend;
        }

        /// <summary>
        /// Lấy danh sách các đầu sách được mượn nhiều nhất (Top Borrowed).
        /// </summary>
        public async Task<List<BookStatDTO>> GetTopBorrowedBooksAsync(DateTime from, DateTime to, int top = 10)
        {
            using var localContext = _context == null ? new LmsDbContext() : null;
            var context = _context ?? localContext!;

            return await context.ChiTietMuonTras
                .Include(c => c.PhieuMuon)
                .Include(c => c.CuonSach)
                .ThenInclude(cs => cs!.Sach)
                .Where(c => c.PhieuMuon!.NgayMuon >= from && c.PhieuMuon!.NgayMuon <= to)
                .GroupBy(c => new { c.CuonSach!.MaSach, c.CuonSach!.Sach!.TenSach })
                .Select(g => new BookStatDTO
                {
                    MaSach = g.Key.MaSach,
                    TenSach = g.Key.TenSach,
                    BorrowCount = g.Count()
                })
                .OrderByDescending(x => x.BorrowCount)
                .Take(top)
                .ToListAsync();
        }

        /// <summary>
        /// Báo cáo độc giả quá hạn hoặc nợ tiền.
        /// </summary>
        public async Task<List<ReaderStatDTO>> GetReaderOverdueReportsAsync()
        {
            using var localContext = _context == null ? new LmsDbContext() : null;
            var context = _context ?? localContext!;

            return await context.DocGias
                .Select(d => new ReaderStatDTO
                {
                    MaDocGia = d.MaDocGia,
                    HoTen = d.HoTen,
                    TongNo = d.TongNo,
                    OverdueCount = context.ChiTietMuonTras
                        .Count(c => c.PhieuMuon!.MaDocGia == d.MaDocGia && 
                                    c.NgayTraThucTe == null && 
                                    c.HanTra < DateTime.Now)
                })
                .Where(x => (double)x.TongNo > 0 || x.OverdueCount > 0)
                .ToListAsync();
        }

        /// <summary>
        /// Báo cáo thống kê mượn sách theo thể loại.
        /// </summary>
        public async Task<IEnumerable<object>> GetBorrowingStatsByCategoryAsync(DateTime from, DateTime to)
        {
            using var localContext = _context == null ? new LmsDbContext() : null;
            var context = _context ?? localContext!;

            var data = await context.ChiTietMuonTras
                .Include(c => c.PhieuMuon)
                .Include(c => c.CuonSach)
                .ThenInclude(cs => cs!.Sach)
                .ThenInclude(s => s!.TheLoaiSach)
                .Where(c => c.PhieuMuon!.NgayMuon >= from && c.PhieuMuon!.NgayMuon <= to)
                .GroupBy(c => c.CuonSach!.Sach!.TheLoaiSach!.TenTheLoai)
                .Select(g => new
                {
                    CategoryName = g.Key,
                    BorrowCount = g.Count()
                })
                .ToListAsync();

            return data;
        }
    }
}
