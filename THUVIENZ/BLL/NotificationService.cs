using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using THUVIENZ.DAL;
using THUVIENZ.Models;

namespace THUVIENZ.BLL
{
    /// <summary>
    /// Service nghiệp vụ xử lý Thông báo của Độc giả.
    /// Tuân thủ nguyên tắc Strict Null Safety và chú thích chi tiết Tiếng Việt.
    /// </summary>
    public class NotificationService
    {
        /// <summary>
        /// Tạo một thông báo mới và lưu xuống CSDL.
        /// </summary>
        public static async Task AddNotificationAsync(string username, string title, string content, string type)
        {
            if (string.IsNullOrEmpty(username)) return;

            using var context = new LmsDbContext();
            var notification = new ThongBao
            {
                TenDangNhap = username,
                TieuDe = title,
                NoiDung = content,
                LoaiThongBao = type,
                NgayThongBao = DateTime.Now,
                DaDoc = false
            };
            context.ThongBaos.Add(notification);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Quét và tự động tạo cảnh báo cho các cuốn sách đang mượn sắp đến hạn trả (còn <= 3 ngày).
        /// Sử dụng mã định danh duy nhất để tránh tạo thông báo trùng lặp.
        /// </summary>
        public static async Task SweepLoomingDueDatesAsync(string username)
        {
            if (string.IsNullOrEmpty(username)) return;

            try
            {
                using var context = new LmsDbContext();
                
                // Tìm thông tin Độc giả tương ứng với tên đăng nhập
                var docGia = await context.DocGias.FirstOrDefaultAsync(d => d.TenDangNhap == username);
                if (docGia == null) return;

                // Tìm tất cả chi tiết mượn chưa được trả
                var activeLoans = await context.ChiTietMuonTras
                    .Include(c => c.CuonSach)
                        .ThenInclude(cs => cs!.Sach)
                    .Include(c => c.PhieuMuon)
                    .Where(c => c.PhieuMuon!.MaDocGia == docGia.MaDocGia && c.NgayTraThucTe == null)
                    .ToListAsync();

                var today = DateTime.Today;
                bool hasNewNoti = false;

                foreach (var loan in activeLoans)
                {
                    // Tính số ngày còn lại đến hạn trả
                    var remainingDays = (loan.HanTra.Date - today).Days;
                    
                    // Nếu thời hạn sắp hết (trong khoảng 0 đến 3 ngày)
                    if (remainingDays >= 0 && remainingDays <= 3)
                    {
                        string tenSach = loan.CuonSach?.Sach?.TenSach ?? "Sách";
                        string uniqueIdentifier = $"cuốn sách '{tenSach}' (Mã phiếu: {loan.MaPhieuMuon})";

                        // Kiểm tra xem đã có cảnh báo hết hạn nào cho cuốn sách và mã phiếu mượn này chưa
                        var exists = await context.ThongBaos.AnyAsync(t =>
                            t.TenDangNhap == username &&
                            t.TieuDe == "Sách sắp hết hạn" &&
                            t.NoiDung.Contains(uniqueIdentifier));

                        if (!exists)
                        {
                            var warningNoti = new ThongBao
                            {
                                TenDangNhap = username,
                                TieuDe = "Sách sắp hết hạn",
                                NoiDung = $"Cảnh báo: Hạn trả cho {uniqueIdentifier} là ngày {loan.HanTra:dd/MM/yyyy} (còn {remainingDays} ngày). Vui lòng trả hoặc gia hạn đúng hạn.",
                                LoaiThongBao = "Warning",
                                NgayThongBao = DateTime.Now,
                                DaDoc = false
                            };
                            context.ThongBaos.Add(warningNoti);
                            hasNewNoti = true;
                        }
                    }
                }

                if (hasNewNoti)
                {
                    await context.SaveChangesAsync();
                }
            }
            catch
            {
                // Bọc try-catch tránh việc lỗi quét làm crash luồng chính của giao diện
            }
        }
    }
}
