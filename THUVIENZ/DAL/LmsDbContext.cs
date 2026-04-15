using Microsoft.EntityFrameworkCore;
using System.Configuration;
using THUVIENZ.Models;

namespace THUVIENZ.DAL
{
    /// <summary>
    /// DbContext chính của ứng dụng sử dụng Entity Framework Core.
    /// Thực hiện ánh xạ (Mapping) giữa các lớp Model C# và các bảng SQL Server.
    /// </summary>
    public class LmsDbContext : DbContext
    {
        public DbSet<Sach> Sachs { get; set; } = null!;
        public DbSet<DocGia> DocGias { get; set; } = null!;
        public DbSet<TaiKhoan> TaiKhoans { get; set; } = null!;
        public DbSet<TheLoaiSach> TheLoaiSachs { get; set; } = null!;
        public DbSet<LoaiDocGia> LoaiDocGias { get; set; } = null!;
        public DbSet<PhieuMuon> PhieuMuons { get; set; } = null!;
        public DbSet<ChiTietPhieuMuon> ChiTietPhieuMuons { get; set; } = null!;
        public DbSet<PhieuTra> PhieuTras { get; set; } = null!;
        public DbSet<ChiTietPhieuTra> ChiTietPhieuTras { get; set; } = null!;
        public DbSet<ThamSo> ThamSos { get; set; } = null!;
        public DbSet<PhieuThuTienPhat> PhieuThuTienPhats { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Lấy chuỗi kết nối từ App.config thông qua ConfigurationManager
                string? connectionString = ConfigurationManager.ConnectionStrings["QL_ThuVien"]?.ConnectionString;
                if (!string.IsNullOrEmpty(connectionString))
                {
                    optionsBuilder.UseSqlServer(connectionString);
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Ánh xạ tên bảng (SQL Server thường dùng tên IN HOA)
            modelBuilder.Entity<Sach>().ToTable("SACH");
            modelBuilder.Entity<DocGia>().ToTable("DOCGIA");
            modelBuilder.Entity<TaiKhoan>().ToTable("TAIKHOAN");
            modelBuilder.Entity<TheLoaiSach>().ToTable("THELOAISACH");
            modelBuilder.Entity<LoaiDocGia>().ToTable("LOAIDOCGIA");
            modelBuilder.Entity<PhieuMuon>().ToTable("PHIEUMUON");
            modelBuilder.Entity<ChiTietPhieuMuon>().ToTable("CHITIETPHIEUMUON");
            modelBuilder.Entity<PhieuTra>().ToTable("PHIEUTRA");
            modelBuilder.Entity<ChiTietPhieuTra>().ToTable("CHITIETPHIEUTRA");
            modelBuilder.Entity<ThamSo>().ToTable("THAMSO");
            modelBuilder.Entity<PhieuThuTienPhat>().ToTable("PHIEUTHUTIENPHAT");

            // 2. Định nghĩa Khóa chính (Primary Keys)
            modelBuilder.Entity<Sach>().HasKey(s => s.MaSach);
            modelBuilder.Entity<DocGia>().HasKey(d => d.MaDocGia);
            modelBuilder.Entity<TaiKhoan>().HasKey(t => t.TenDangNhap);
            modelBuilder.Entity<TheLoaiSach>().HasKey(t => t.MaTheLoai);
            modelBuilder.Entity<LoaiDocGia>().HasKey(l => l.MaLoaiDocGia);
            modelBuilder.Entity<PhieuMuon>().HasKey(p => p.MaPhieuMuon);
            modelBuilder.Entity<PhieuTra>().HasKey(p => p.MaPhieuTra);
            modelBuilder.Entity<ThamSo>().HasKey(t => t.TenThamSo);
            modelBuilder.Entity<PhieuThuTienPhat>().HasKey(p => p.MaPhieuThu);

            // 3. Định nghĩa Khóa phức hợp (Composite Keys cho các bảng Chi Tiết)
            modelBuilder.Entity<ChiTietPhieuMuon>()
                .HasKey(c => new { c.MaPhieuMuon, c.MaSach });

            modelBuilder.Entity<ChiTietPhieuTra>()
                .HasKey(c => new { c.MaPhieuTra, c.MaPhieuMuon, c.MaSach });
        }
    }
}
