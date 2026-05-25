using Microsoft.EntityFrameworkCore;
using System.Configuration;
using THUVIENZ.Models;

namespace THUVIENZ.DAL
{
    /// <summary>
    /// DbContext chính của ứng dụng sử dụng Entity Framework Core 8.
    /// Thực hiện ánh xạ (Mapping) chuẩn hóa giữa các lớp Model C# và các bảng SQL Server mới.
    /// Toàn bộ code áp dụng Strict Null Safety và chú thích Tiếng Việt chi tiết.
    /// </summary>
    public class LmsDbContext : DbContext
    {
        public LmsDbContext()
        {
        }

        public LmsDbContext(DbContextOptions<LmsDbContext> options) : base(options)
        {
        }

        public DbSet<Sach> Sachs { get; set; } = null!;
        public DbSet<CuonSach> CuonSachs { get; set; } = null!;
        public DbSet<DocGia> DocGias { get; set; } = null!;
        public DbSet<TaiKhoan> TaiKhoans { get; set; } = null!;
        public DbSet<TheLoaiSach> TheLoaiSachs { get; set; } = null!;
        public DbSet<LoaiDocGia> LoaiDocGias { get; set; } = null!;
        public DbSet<PhieuMuon> PhieuMuons { get; set; } = null!;
        public DbSet<ChiTietMuonTra> ChiTietMuonTras { get; set; } = null!;
        public DbSet<ThamSo> ThamSos { get; set; } = null!;
        public DbSet<PhieuThuTienPhat> PhieuThuTienPhats { get; set; } = null!;
        public DbSet<SachYeuThich> SachYeuThichs { get; set; } = null!;
        public DbSet<YeuCauMuon> YeuCauMuons { get; set; } = null!;
        public DbSet<ThongBao> ThongBaos { get; set; } = null!;

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

            // ====================================================================
            // 1. ÁNH XẠ TÊN BẢNG (TABLE MAPPINGS)
            // ====================================================================
            modelBuilder.Entity<Sach>().ToTable("SACH");
            modelBuilder.Entity<CuonSach>().ToTable("CUONSACH");
            modelBuilder.Entity<DocGia>().ToTable("DOCGIA");
            modelBuilder.Entity<TaiKhoan>().ToTable("TAIKHOAN");
            modelBuilder.Entity<TheLoaiSach>().ToTable("THELOAISACH");
            modelBuilder.Entity<LoaiDocGia>().ToTable("LOAIDOCGIA");
            modelBuilder.Entity<PhieuMuon>().ToTable("PHIEUMUON");
            modelBuilder.Entity<ChiTietMuonTra>().ToTable("CHITIETMUONTRA", tb => tb.HasTrigger("trg_SyncCuonSachStatus"));
            modelBuilder.Entity<ThamSo>().ToTable("THAMSO");
            modelBuilder.Entity<PhieuThuTienPhat>().ToTable("PHIEUTHUTIENPHAT");
            modelBuilder.Entity<SachYeuThich>().ToTable("SACHYEUTHICH");
            modelBuilder.Entity<YeuCauMuon>().ToTable("YEUCAUMUON");
            modelBuilder.Entity<ThongBao>().ToTable("THONGBAO");

            // ====================================================================
            // 2. ĐỊNH NGHĨA KHÓA CHÍNH (PRIMARY KEYS)
            // ====================================================================
            modelBuilder.Entity<Sach>().HasKey(s => s.MaSach);
            modelBuilder.Entity<CuonSach>().HasKey(c => c.MaCuonSach);
            modelBuilder.Entity<DocGia>().HasKey(d => d.MaDocGia);
            modelBuilder.Entity<TaiKhoan>().HasKey(t => t.TenDangNhap);
            modelBuilder.Entity<TheLoaiSach>().HasKey(t => t.MaTheLoai);
            modelBuilder.Entity<LoaiDocGia>().HasKey(l => l.MaLoaiDocGia);
            modelBuilder.Entity<PhieuMuon>().HasKey(p => p.MaPhieuMuon);
            modelBuilder.Entity<ThamSo>().HasKey(t => t.TenThamSo);
            modelBuilder.Entity<PhieuThuTienPhat>().HasKey(p => p.MaPhieuThu);
            modelBuilder.Entity<YeuCauMuon>().HasKey(y => y.MaYeuCau);
            modelBuilder.Entity<ThongBao>().HasKey(t => t.MaThongBao);
            modelBuilder.Entity<SachYeuThich>().HasKey(s => new { s.MaDocGia, s.MaSach });

            // Cấu hình Khóa chính phức hợp (Composite Key) cho bảng gộp mượn trả
            modelBuilder.Entity<ChiTietMuonTra>()
                .HasKey(c => new { c.MaPhieuMuon, c.MaCuonSach });

            // Cấu hình trường RowVersion hỗ trợ Optimistic Concurrency Control (OCC)
            modelBuilder.Entity<Sach>()
                .Property(s => s.RowVersion)
                .IsRowVersion();

            // ====================================================================
            // 3. CẤU HÌNH CÁC MỐI QUAN HỆ (RELATIONSHIPS & FOREIGN KEYS)
            // ====================================================================

            // Quan hệ: Độc Giả - Loại Độc Giả (N-1)
            modelBuilder.Entity<DocGia>()
                .HasOne(d => d.LoaiDocGia)
                .WithMany(l => l.DocGias)
                .HasForeignKey(d => d.MaLoaiDocGia)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Quan hệ: Độc Giả - Tài Khoản (1-1)
            modelBuilder.Entity<DocGia>()
                .HasOne(d => d.TaiKhoan)
                .WithOne(t => t.DocGia)
                .HasForeignKey<DocGia>(d => d.TenDangNhap)
                .OnDelete(DeleteBehavior.SetNull);

            // Quan hệ: Đầu Sách - Thể Loại Sách (N-1)
            modelBuilder.Entity<Sach>()
                .HasOne(s => s.TheLoaiSach)
                .WithMany(t => t.Sachs)
                .HasForeignKey(s => s.MaTheLoai)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Quan hệ: Cuốn Sách vật lý - Đầu Sách gốc (N-1) với Cascade Delete
            modelBuilder.Entity<CuonSach>()
                .HasOne(c => c.Sach)
                .WithMany(s => s.CuonSachs)
                .HasForeignKey(c => c.MaSach)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ: Phiếu Mượn - Độc Giả (N-1)
            modelBuilder.Entity<PhieuMuon>()
                .HasOne(p => p.DocGia)
                .WithMany(d => d.PhieuMuons)
                .HasForeignKey(p => p.MaDocGia)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ: Chi Tiết Mượn Trả - Phiếu Mượn (N-1) với Cascade Delete
            modelBuilder.Entity<ChiTietMuonTra>()
                .HasOne(c => c.PhieuMuon)
                .WithMany(p => p.ChiTietMuonTras)
                .HasForeignKey(c => c.MaPhieuMuon)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ: Chi Tiết Mượn Trả - Cuốn Sách vật lý (N-1)
            modelBuilder.Entity<ChiTietMuonTra>()
                .HasOne(c => c.CuonSach)
                .WithMany(cs => cs.ChiTietMuonTras)
                .HasForeignKey(c => c.MaCuonSach)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Quan hệ: Phiếu Thu Tiền Phạt - Độc Giả (N-1)
            modelBuilder.Entity<PhieuThuTienPhat>()
                .HasOne(p => p.DocGia)
                .WithMany(d => d.PhieuThuTienPhats)
                .HasForeignKey(p => p.MaDocGia)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ: Sách Yêu Thích 
            modelBuilder.Entity<SachYeuThich>()
                .HasOne(s => s.DocGia)
                .WithMany()
                .HasForeignKey(s => s.MaDocGia)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SachYeuThich>()
                .HasOne(s => s.Sach)
                .WithMany()
                .HasForeignKey(s => s.MaSach)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ: Yêu cầu mượn
            modelBuilder.Entity<YeuCauMuon>()
                .HasOne(y => y.DocGia)
                .WithMany()
                .HasForeignKey(y => y.MaDocGia)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<YeuCauMuon>()
                .HasOne(y => y.Sach)
                .WithMany()
                .HasForeignKey(y => y.MaSach)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ: Thông báo - Tài khoản
            modelBuilder.Entity<ThongBao>()
                .HasOne(t => t.TaiKhoan)
                .WithMany()
                .HasForeignKey(t => t.TenDangNhap)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
