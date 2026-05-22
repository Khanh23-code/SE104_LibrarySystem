-- ======================================================================
-- DATABASE: QL_THU_VIEN (MASTER VERSION FOR 3-DAY MVP)
-- Tối ưu cho mô hình Local-First C# & Kiosk tự phục vụ
-- ======================================================================
USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'QL_ThuVien')
BEGIN
    ALTER DATABASE QL_ThuVien SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QL_ThuVien;
END;
GO

CREATE DATABASE QL_ThuVien;
GO
USE QL_ThuVien;
GO

-- 1. TÀI KHOẢN (Đã khóa cứng 2 Role: Admin và Reader, kèm trạng thái)
CREATE TABLE TAIKHOAN (
    TenDangNhap VARCHAR(50) PRIMARY KEY,
    MatKhau VARCHAR(255) NOT NULL, 
    Quyen NVARCHAR(20) NOT NULL CHECK (Quyen IN ('Admin', 'Reader')), 
    TrangThai NVARCHAR(20) DEFAULT 'Pending' CHECK (TrangThai IN ('Pending', 'Active', 'Locked', 'DisActive'))
);

-- 2. THAM SỐ (Cấu hình rule hệ thống linh hoạt)
CREATE TABLE THAMSO (
    TenThamSo VARCHAR(50) PRIMARY KEY,
    GiaTri FLOAT NOT NULL
);

-- 3. LOẠI ĐỘC GIẢ
CREATE TABLE LOAIDOCGIA (
    MaLoaiDocGia INT PRIMARY KEY IDENTITY(1,1),
    TenLoaiDocGia NVARCHAR(50) NOT NULL
);

-- 4. ĐỘC GIẢ (Bổ sung chuẩn UI: Giới tính, SĐT)
CREATE TABLE DOCGIA (
    MaDocGia INT PRIMARY KEY IDENTITY(1,1),
    TenDangNhap VARCHAR(50) UNIQUE, -- Map 1-1 với TAIKHOAN
    HoTen NVARCHAR(100) NOT NULL,
    MaLoaiDocGia INT,
    GioiTinh NVARCHAR(10), 
    SoDienThoai VARCHAR(15),
    Email NVARCHAR(100),
    DiaChi NVARCHAR(200),
    NgaySinh DATE,
    NgayLapThe DATE DEFAULT GETDATE(),
    TongNo MONEY DEFAULT 0,
    AnhDaiDien NVARCHAR(500),
    FOREIGN KEY (MaLoaiDocGia) REFERENCES LOAIDOCGIA(MaLoaiDocGia),
    FOREIGN KEY (TenDangNhap) REFERENCES TAIKHOAN(TenDangNhap) ON DELETE SET NULL
);

-- 5. THỂ LOẠI SÁCH
CREATE TABLE THELOAISACH (
    MaTheLoai INT PRIMARY KEY IDENTITY(1,1),
    TenTheLoai NVARCHAR(50) NOT NULL
);

-- 6. ĐẦU SÁCH (Thông tin chung - Khớp 100% với form "Thêm sách mới")
CREATE TABLE SACH (
    MaSach INT PRIMARY KEY IDENTITY(1,1),
    MaISBN VARCHAR(50) UNIQUE, -- Tương ứng "Mã ID Sách" trên UI
    TenSach NVARCHAR(100) NOT NULL,
    MaTheLoai INT,
    TacGia NVARCHAR(100),
    NhaXuatBan NVARCHAR(100),
    NamXuatBan INT,
    NgonNgu NVARCHAR(50) DEFAULT N'Tiếng Việt',
    TriGia MONEY,
    MoTa NVARCHAR(500),
    HinhAnh NVARCHAR(255), -- Lưu đường dẫn ảnh local
    RowVersion ROWVERSION, -- Optimistic Concurrency Control
    FOREIGN KEY (MaTheLoai) REFERENCES THELOAISACH(MaTheLoai)
);

-- 7. CUỐN SÁCH (Bản sao vật lý - Dùng để Kiosk quét Barcode/RFID)
-- Giải thích: Khi Admin nhập UI "Số lượng = 5", C# sẽ tạo 1 record [SACH] và insert 5 record [CUONSACH]
CREATE TABLE CUONSACH (
    MaCuonSach INT PRIMARY KEY IDENTITY(1,1), -- Đây chính là mã dán trên gáy từng quyển sách
    MaSach INT NOT NULL,
    TinhTrang NVARCHAR(50) DEFAULT N'Sẵn sàng' CHECK (TinhTrang IN (N'Sẵn sàng', N'Đang mượn', N'Bị mất', N'Bảo trì')),
    NgayNhap DATE DEFAULT GETDATE(),
    FOREIGN KEY (MaSach) REFERENCES SACH(MaSach) ON DELETE CASCADE
);

-- 8. PHIẾU MƯỢN (Lưu vết Giao dịch chung)
CREATE TABLE PHIEUMUON (
    MaPhieuMuon INT PRIMARY KEY IDENTITY(1,1),
    MaDocGia INT NOT NULL,
    NgayMuon DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaDocGia) REFERENCES DOCGIA(MaDocGia) ON DELETE CASCADE
);

-- 9. CHI TIẾT MƯỢN TRẢ (Đã GỘP logic Mượn và Trả làm một)
-- Giải thích: Khi mượn -> NgayTraThucTe = NULL. Khi quét trả -> Update NgayTraThucTe & tính TienPhat.
CREATE TABLE CHITIETMUONTRA (
    MaPhieuMuon INT,
    MaCuonSach INT,
    HanTra DATETIME NOT NULL, -- Tính trước bằng C# (NgayMuon + ThamSo) ghi thẳng vào đây
    NgayTraThucTe DATETIME NULL, 
    TienPhat MONEY DEFAULT 0,
    TinhTrangCuonSachKhiTra NVARCHAR(50) NULL,
    PRIMARY KEY (MaPhieuMuon, MaCuonSach),
    FOREIGN KEY (MaPhieuMuon) REFERENCES PHIEUMUON(MaPhieuMuon) ON DELETE CASCADE,
    FOREIGN KEY (MaCuonSach) REFERENCES CUONSACH(MaCuonSach)
);

-- 10. PHIẾU THU TIỀN PHẠT
CREATE TABLE PHIEUTHUTIENPHAT (
    MaPhieuThu INT PRIMARY KEY IDENTITY(1,1),
    MaDocGia INT NOT NULL,
    SoTienThu MONEY NOT NULL,
    NgayThu DATETIME DEFAULT GETDATE(),
    GhiChu NVARCHAR(200),
    FOREIGN KEY (MaDocGia) REFERENCES DOCGIA(MaDocGia) ON DELETE CASCADE
);
GO

-- 11. SÁCH YÊU THÍCH
CREATE TABLE SACHYEUTHICH (
    MaDocGia INT,
    MaSach INT,
    NgayThem DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (MaDocGia, MaSach),
    FOREIGN KEY (MaDocGia) REFERENCES DOCGIA(MaDocGia) ON DELETE CASCADE,
    FOREIGN KEY (MaSach) REFERENCES SACH(MaSach) ON DELETE CASCADE
);
GO

-- 12. YÊU CẦU MƯỢN (YEUCAUMUON) - bảng lưu yêu cầu mượn sách của độc giả
CREATE TABLE YEUCAUMUON (
    MaYeuCau INT PRIMARY KEY IDENTITY(1,1),
    MaDocGia INT NOT NULL,
    MaSach INT NOT NULL,
    NgayYeuCau DATETIME DEFAULT GETDATE(),
    TrangThai NVARCHAR(20) DEFAULT N'Pending' CHECK (TrangThai IN (N'Pending', N'Notified', N'Cancelled')),
    FOREIGN KEY (MaDocGia) REFERENCES DOCGIA(MaDocGia) ON DELETE CASCADE,
    FOREIGN KEY (MaSach) REFERENCES SACH(MaSach) ON DELETE CASCADE
);
GO

-- ======================================================================
-- BỘ DỮ LIỆU KHỞI TẠO MẶC ĐỊNH
-- ======================================================================
INSERT INTO TAIKHOAN (TenDangNhap, MatKhau, Quyen, TrangThai) VALUES ('admin', 'admin123', 'Admin', 'Active');
-- Thêm tài khoản reader mẫu để test (username: reader1, password: 123456)
INSERT INTO TAIKHOAN (TenDangNhap, MatKhau, Quyen, TrangThai) VALUES ('reader1', '123456', 'Reader', 'Active');
INSERT INTO THAMSO (TenThamSo, GiaTri) VALUES ('SoNgayMuonToiDa', 14);
INSERT INTO THAMSO (TenThamSo, GiaTri) VALUES ('TienPhatMoiNgay', 2000);
INSERT INTO THAMSO (TenThamSo, GiaTri) VALUES ('SoSachMuonToiDa', 5);
GO

-- Khởi tạo danh mục Thể loại sách mặc định
SET IDENTITY_INSERT THELOAISACH ON;
INSERT INTO THELOAISACH (MaTheLoai, TenTheLoai) VALUES (1, N'Khoa học Công nghệ');
INSERT INTO THELOAISACH (MaTheLoai, TenTheLoai) VALUES (2, N'Văn học Nghệ thuật');
INSERT INTO THELOAISACH (MaTheLoai, TenTheLoai) VALUES (3, N'Kinh tế & Quản trị');
SET IDENTITY_INSERT THELOAISACH OFF;
GO

-- Khởi tạo danh mục Loại độc giả mặc định
SET IDENTITY_INSERT LOAIDOCGIA ON;
INSERT INTO LOAIDOCGIA (MaLoaiDocGia, TenLoaiDocGia) VALUES (1, N'Sinh viên');
INSERT INTO LOAIDOCGIA (MaLoaiDocGia, TenLoaiDocGia) VALUES (2, N'Giảng viên');
SET IDENTITY_INSERT LOAIDOCGIA OFF;
GO

-- ======================================================================
-- DỮ LIỆU MẪU CHO PHÁT TRIỂN VÀ KIỂM THỬ (Profile, Search, Borrowing)
-- ======================================================================
-- Thêm 1 độc giả mẫu liên kết với tài khoản 'reader1'
INSERT INTO DOCGIA (TenDangNhap, HoTen, MaLoaiDocGia, GioiTinh, SoDienThoai, Email, DiaChi, NgaySinh, AnhDaiDien)
VALUES ('reader1', N'Người đọc mẫu', 1, N'Nam', '0123456789', 'reader1@example.com', N'Hà Nội', '1995-01-01', '/images/readers/reader1.png');
GO

-- Thêm một vài đầu sách mẫu và các bản sao vật lý tương ứng để trang Search/Borrowing có dữ liệu
DECLARE @s1 INT, @s2 INT, @s3 INT;
INSERT INTO SACH (MaISBN, TenSach, MaTheLoai, TacGia, NhaXuatBan, NamXuatBan, NgonNgu, TriGia, MoTa, HinhAnh)
VALUES ('ISBN001', N'Lập trình C# cho người mới', 1, N'Tác giả A', N'NXB A', 2022, N'Tiếng Việt', 120000, N'Hướng dẫn cơ bản C#', '/images/books/book1.png');
SET @s1 = SCOPE_IDENTITY();
INSERT INTO SACH (MaISBN, TenSach, MaTheLoai, TacGia, NhaXuatBan, NamXuatBan, NgonNgu, TriGia, MoTa, HinhAnh)
VALUES ('ISBN002', N'Hướng dẫn WPF', 1, N'Tác giả B', N'NXB B', 2021, N'Tiếng Việt', 100000, N'Hướng dẫn xây dựng ứng dụng WPF', '/images/books/book2.png');
SET @s2 = SCOPE_IDENTITY();
INSERT INTO SACH (MaISBN, TenSach, MaTheLoai, TacGia, NhaXuatBan, NamXuatBan, NgonNgu, TriGia, MoTa, HinhAnh)
VALUES ('ISBN003', N'Thực hành Thuật toán', 3, N'Tác giả C', N'NXB C', 2019, N'English', 150000, N'Thực hành thuật toán cơ bản', '/images/books/book3.png');
SET @s3 = SCOPE_IDENTITY();
GO

-- Thêm bản sao vật lý (CuonSach) cho các đầu sách trên
INSERT INTO CUONSACH (MaSach, TinhTrang) VALUES (@s1, N'Sẵn sàng');
INSERT INTO CUONSACH (MaSach, TinhTrang) VALUES (@s1, N'Sẵn sàng');
INSERT INTO CUONSACH (MaSach, TinhTrang) VALUES (@s2, N'Sẵn sàng');
INSERT INTO CUONSACH (MaSach, TinhTrang) VALUES (@s3, N'Đang mượn');
GO

-- Đánh dấu 1 cuốn là sách yêu thích của độc giả mẫu
INSERT INTO SACHYEUTHICH (MaDocGia, MaSach)
VALUES ((SELECT MaDocGia FROM DOCGIA WHERE TenDangNhap = 'reader1'), @s1);
GO

-- Thêm 1 yêu cầu mượn mẫu (Pending)
INSERT INTO YEUCAUMUON (MaDocGia, MaSach, TrangThai)
VALUES ((SELECT MaDocGia FROM DOCGIA WHERE TenDangNhap = 'reader1'), @s2, N'Pending');
GO

IF NOT EXISTS (
    SELECT 1 FROM THAMSO WHERE TenThamSo = N'TongNoToiDa'
)
BEGIN
    INSERT INTO THAMSO (TenThamSo, GiaTri) VALUES (N'TongNoToiDa', 50000);
END
GO
