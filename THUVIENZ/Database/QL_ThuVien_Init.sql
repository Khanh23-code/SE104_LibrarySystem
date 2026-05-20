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