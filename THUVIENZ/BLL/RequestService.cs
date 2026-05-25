using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using THUVIENZ.DAL;
using THUVIENZ.Models;

namespace THUVIENZ.BLL
{
    public class RequestService
    {
        public async Task<bool> AddRequestAsync(int maDocGia, int maSach)
        {
            using var context = new LmsDbContext();
            var exists = await context.YeuCauMuons.AnyAsync(y => y.MaDocGia == maDocGia && y.MaSach == maSach && y.TrangThai == "Pending");
            if (exists) return false;
            context.YeuCauMuons.Add(new YeuCauMuon { MaDocGia = maDocGia, MaSach = maSach });
            await context.SaveChangesAsync();

            // Sinh thông báo tự động gửi yêu cầu mượn thành công
            var docGia = await context.DocGias.FindAsync(maDocGia);
            var sach = await context.Sachs.FindAsync(maSach);
            if (docGia != null && sach != null && !string.IsNullOrEmpty(docGia.TenDangNhap))
            {
                var notification = new ThongBao
                {
                    TenDangNhap = docGia.TenDangNhap,
                    TieuDe = "Gửi yêu cầu mượn sách",
                    NoiDung = $"Yêu cầu mượn cuốn sách '{sach.TenSach}' đã được gửi thành công và đang chờ thủ thư phê duyệt.",
                    LoaiThongBao = "Info",
                    NgayThongBao = System.DateTime.Now,
                    DaDoc = false
                };
                context.ThongBaos.Add(notification);
                await context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> CancelRequestAsync(int maYeuCau)
        {
            using var context = new LmsDbContext();
            var r = await context.YeuCauMuons.FindAsync(maYeuCau);
            if (r == null) return false;
            r.TrangThai = "Cancelled";
            await context.SaveChangesAsync();
            return true;
        }
    }
}
