using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using THUVIENZ.DAL;
using THUVIENZ.DAL.Base;
using THUVIENZ.Models;

namespace THUVIENZ.BLL
{
    /// <summary>
    /// Lớp chứa kết quả trả sách hỗ trợ hiển thị chi tiết lên giao diện.
    /// </summary>
    public class KetQuaTraSach
    {
        public bool ThanhCong { get; set; }
        public string ThongBao { get; set; } = string.Empty;
        public decimal TienPhat { get; set; }
        public int SoNgayTre { get; set; }
    }


    /// <summary>
    /// Service nghiệp vụ xử lý Mượn và Trả sách tập trung theo cấu trúc DB mới gộp chung.
    /// Tuân thủ nguyên tắc Strict Null Safety và chú thích 100% Tiếng Việt.
    /// </summary>
    public class MuonTraService
    {
        public MuonTraService()
        {
        }

        public MuonTraService(LmsDbContext context, LibrarySettingsService settingsService)
        {
        }

        /// <summary>
        /// Gửi yêu cầu trả một cuốn sách từ phía Reader (Kiosk).
        /// Chỉ cập nhật tình trạng thành "Yêu cầu trả" chứ chưa chính thức hoàn trả.
        /// </summary>
        public async Task<bool> YeuCauTraSachAsync(int maCuonSach, int maPhieuMuon)
        {
            using var context = new LmsDbContext();
            
            var chiTiet = await context.ChiTietMuonTras
                .FirstOrDefaultAsync(c => c.MaCuonSach == maCuonSach && c.MaPhieuMuon == maPhieuMuon && c.NgayTraThucTe == null && c.TinhTrangCuonSachKhiTra != "Yêu cầu trả");

            if (chiTiet == null)
            {
                throw new InvalidOperationException("Không tìm thấy bản ghi mượn chưa trả cho cuốn sách này.");
            }

            chiTiet.TinhTrangCuonSachKhiTra = "Yêu cầu trả";
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Thực hiện thủ tục hoàn trả một cuốn sách vật lý dựa trên mã cuốn sách (RFID/Barcode).
        /// Bọc toàn bộ trong Database Transaction để đảm bảo tính nguyên tử tuyệt đối.
        /// </summary>
        public async Task<KetQuaTraSach> ThucHienTraSachAsync(int maCuonSach, int maPhieuMuon = 0)
        {
            using var context = new LmsDbContext();
            var settingsService = new LibrarySettingsService(new BaseRepository<ThamSo>(context));
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tìm bản ghi trong CHITIETMUONTRA khớp mã cuốn sách và chưa được trả (NgayTraThucTe == null)
                ChiTietMuonTra? chiTiet = null;
                if (maPhieuMuon > 0)
                {
                    chiTiet = await context.ChiTietMuonTras
                        .Include(c => c.CuonSach)
                        .Include(c => c.PhieuMuon)
                        .ThenInclude(p => p!.DocGia)
                        .FirstOrDefaultAsync(c => c.MaCuonSach == maCuonSach && c.MaPhieuMuon == maPhieuMuon && c.NgayTraThucTe == null);
                }

                if (chiTiet == null)
                {
                    chiTiet = await context.ChiTietMuonTras
                        .Include(c => c.CuonSach)
                        .Include(c => c.PhieuMuon)
                        .ThenInclude(p => p!.DocGia)
                        .FirstOrDefaultAsync(c => c.MaCuonSach == maCuonSach && c.NgayTraThucTe == null);
                }

                // Nếu không tìm thấy, ném ra ngoại lệ cảnh báo chính xác theo yêu cầu
                if (chiTiet == null)
                {
                    throw new InvalidOperationException("Cuốn sách này không nằm trong danh sách đang mượn");
                }

                // 2. Cập nhật ngày trả thực tế là thời điểm hiện tại
                DateTime ngayTra = DateTime.Now;
                chiTiet.NgayTraThucTe = ngayTra;
                chiTiet.TinhTrangCuonSachKhiTra = "Sẵn sàng";

                // 3. Tính toán Tiền phạt nếu trả trễ hạn
                decimal tienPhat = 0;
                int soNgayTre = 0;

                // So sánh ngày (bỏ qua giờ phút) để xác định chính xác số ngày trễ
                if (ngayTra.Date > chiTiet.HanTra.Date)
                {
                    soNgayTre = (ngayTra.Date - chiTiet.HanTra.Date).Days;
                    
                    // Lấy đơn giá phạt mỗi ngày từ bảng THAMSO thông qua SettingsService
                    decimal donGiaPhat = (decimal)await settingsService.GetValueAsync("TienPhatMoiNgay");
                    tienPhat = soNgayTre * donGiaPhat;
                }

                chiTiet.TienPhat = tienPhat;

                // 5. Nếu phát sinh tiền phạt, cộng dồn vào Tổng nợ của Độc giả và tự động kiểm tra ngưỡng đình chỉ
                bool biDinhChi = false;
                if (tienPhat > 0 && chiTiet.PhieuMuon?.DocGia != null)
                {
                    var docGia = chiTiet.PhieuMuon.DocGia;
                    docGia.TongNo += tienPhat;

                    // Lấy ra ngưỡng nợ đọng tối đa từ bảng THAMSO
                    decimal tongNoToiDa = (decimal)await settingsService.GetValueAsync("TongNoToiDa");
                    
                    // Tự động kiểm tra nếu Tổng nợ vượt ngưỡng thì đình chỉ (Khóa) tài khoản
                    if (docGia.TongNo > tongNoToiDa)
                    {
                        var taiKhoan = await context.TaiKhoans.FirstOrDefaultAsync(t => t.TenDangNhap == docGia.TenDangNhap);
                        if (taiKhoan != null && taiKhoan.TrangThai == "Active")
                        {
                            taiKhoan.TrangThai = "Locked";
                            biDinhChi = true;
                        }
                    }
                }

                // Lưu toàn bộ thay đổi xuống DB
                await context.SaveChangesAsync();

                // Xác nhận giao dịch thành công
                await transaction.CommitAsync();

                return new KetQuaTraSach
                {
                    ThanhCong = true,
                    ThongBao = tienPhat > 0 
                        ? (biDinhChi 
                            ? $"Trả sách thành công! Trễ hạn {soNgayTre} ngày, phạt: {tienPhat:N0} VNĐ.\n⚠️ CẢNH BÁO: Tổng nợ vượt ngưỡng cho phép, tài khoản độc giả tự động bị đình chỉ (Khóa)!"
                            : $"Trả sách thành công! Trễ hạn {soNgayTre} ngày, phát sinh phạt: {tienPhat:N0} VNĐ.")
                        : "Trả sách thành công đúng hạn!",
                    TienPhat = tienPhat,
                    SoNgayTre = soNgayTre
                };
            }
            catch
            {
                // Hoàn tác nếu xảy ra bất kỳ lỗi nào
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Thực hiện thủ tục mượn danh sách các cuốn sách vật lý cho một Độc giả.
        /// </summary>
        /// <summary>
        /// Gia hạn hạn trả của một cuốn sách vật lý đang được mượn.
        /// Tìm bản ghi ChiTietMuonTra chưa trả và cộng thêm SoNgayMuonToiDa vào HanTra hiện tại.
        /// </summary>
        public async Task<bool> GiaHanSachAsync(int maCuonSach)
        {
            using var context = new LmsDbContext();
            var settingsService = new LibrarySettingsService(new BaseRepository<ThamSo>(context));
            var chiTiet = await context.ChiTietMuonTras
                .Include(c => c.PhieuMuon)
                .FirstOrDefaultAsync(c => c.MaCuonSach == maCuonSach && c.NgayTraThucTe == null);

            if (chiTiet == null)
                throw new InvalidOperationException("Không tìm thấy bản ghi mượn đang hoạt động cho cuốn sách này.");

            if (chiTiet.HanTra < DateTime.Now)
                throw new InvalidOperationException("Không thể gia hạn vì sách đã quá hạn.");

            int soNgayMuonToiDa = (int)await settingsService.GetValueAsync("SoNgayMuonToiDa");
            int currentDays = (int)Math.Round((chiTiet.HanTra - chiTiet.PhieuMuon!.NgayMuon).TotalDays);
            int extensions = (currentDays - soNgayMuonToiDa) / 14;

            if (extensions >= 2)
                throw new InvalidOperationException("Đã đạt số lần gia hạn tối đa (2 lần).");

            chiTiet.HanTra = chiTiet.HanTra.AddDays(14);

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ThucHienMuonSachAsync(int maDocGia, List<int> danhSachMaCuonSach)
        {
            if (danhSachMaCuonSach == null || danhSachMaCuonSach.Count == 0)
                throw new ArgumentException("Danh sách cuốn sách mượn không được rỗng.");

            using var context = new LmsDbContext();
            var settingsService = new LibrarySettingsService(new BaseRepository<ThamSo>(context));
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // 1. Kiểm tra giới hạn số sách mượn tối đa của độc giả
                int soSachToiDa = (int)await settingsService.GetValueAsync("SoSachMuonToiDa");
                int soNgayMuonToiDa = (int)await settingsService.GetValueAsync("SoNgayMuonToiDa");

                // Đếm số sách vật lý độc giả đang mượn chưa trả
                int soSachDangMuon = await context.ChiTietMuonTras
                    .Include(c => c.PhieuMuon)
                    .CountAsync(c => c.PhieuMuon!.MaDocGia == maDocGia && c.NgayTraThucTe == null);

                if (soSachDangMuon + danhSachMaCuonSach.Count > soSachToiDa)
                {
                    throw new InvalidOperationException($"Độc giả đã vượt quá hạn mức mượn tối đa ({soSachToiDa} cuốn). Hiện đang mượn {soSachDangMuon} cuốn.");
                }

                // 2. Tạo Phiếu Mượn mới
                var phieuMuon = new PhieuMuon
                {
                    MaDocGia = maDocGia,
                    NgayMuon = DateTime.Now
                };

                await context.PhieuMuons.AddAsync(phieuMuon);
                await context.SaveChangesAsync(); // Lưu để lấy MaPhieuMuon tự tăng

                // 3. Xử lý từng cuốn sách vật lý
                DateTime hanTra = DateTime.Now.AddDays(soNgayMuonToiDa);

                foreach (int maCuonSach in danhSachMaCuonSach)
                {
                    var cuonSach = await context.CuonSachs
                        .Include(c => c.Sach)
                        .FirstOrDefaultAsync(c => c.MaCuonSach == maCuonSach);

                    if (cuonSach == null)
                        throw new InvalidOperationException($"Không tìm thấy cuốn sách vật lý với mã: {maCuonSach}");

                    if (cuonSach.TinhTrang != "Sẵn sàng")
                        throw new InvalidOperationException($"Cuốn sách '{cuonSach.Sach?.TenSach}' (Mã: {maCuonSach}) hiện đang ở trạng thái '{cuonSach.TinhTrang}', không thể mượn.");

                    // Tạo chi tiết mượn trả
                    var chiTiet = new ChiTietMuonTra
                    {
                        MaPhieuMuon = phieuMuon.MaPhieuMuon,
                        MaCuonSach = maCuonSach,
                        HanTra = hanTra,
                        NgayTraThucTe = null,
                        TienPhat = 0
                    };

                    await context.ChiTietMuonTras.AddAsync(chiTiet);
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
