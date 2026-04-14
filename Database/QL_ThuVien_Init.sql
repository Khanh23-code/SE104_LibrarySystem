-- =========================================================
-- DATABASE: QL_THU_VIEN (FINAL VERSION FOR DEV)
-- =========================================================
CREATE DATABASE QL_ThuVien;
GO
USE QL_ThuVien;
GO

-- 1. TAIKHOAN: Tăng độ dài MatKhau để lưu Hash 
CREATE TABLE TAIKHOAN (
    TenDangNhap VARCHAR(50) PRIMARY KEY,
    MatKhau VARCHAR(255) NOT NULL, 
    Quyen NVARCHAR(20) NOT NULL -- 'Admin' hoặc 'Reader'
);

-- 2. THAMSO: Lưu các quy định của thư viện
CREATE TABLE THAMSO (
    TenThamSo VARCHAR(50) PRIMARY KEY,
    GiaTri FLOAT NOT NULL
);

-- 3. LOAIDOCGIA
CREATE TABLE LOAIDOCGIA (
    MaLoaiDocGia INT PRIMARY KEY IDENTITY(1,1),
    TenLoaiDocGia NVARCHAR(50) NOT NULL
);

-- 4. DOCGIA: Fix ràng buộc UNIQUE cho tài khoản 
CREATE TABLE DOCGIA (
    MaDocGia INT PRIMARY KEY IDENTITY(1,1),
    HoTen NVARCHAR(100) NOT NULL,
    MaLoaiDocGia INT,
    NgaySinh DATE,
    DiaChi NVARCHAR(200),
    Email NVARCHAR(100),
    NgayLapThe DATE,
    TongNo MONEY DEFAULT 0,
    TenDangNhap VARCHAR(50) UNIQUE, -- Đảm bảo 1-1
    FOREIGN KEY (MaLoaiDocGia) REFERENCES LOAIDOCGIA(MaLoaiDocGia),
    FOREIGN KEY (TenDangNhap) REFERENCES TAIKHOAN(TenDangNhap)
);

-- 5. THELOAISACH
CREATE TABLE THELOAISACH (
    MaTheLoai INT PRIMARY KEY IDENTITY(1,1),
    TenTheLoai NVARCHAR(50) NOT NULL
);

-- 6. SACH
CREATE TABLE SACH (
    MaSach INT PRIMARY KEY IDENTITY(1,1),
    TenSach NVARCHAR(100) NOT NULL,
    MaTheLoai INT,
    TacGia NVARCHAR(100),
    NamXuatBan INT,
    NhaXuatBan NVARCHAR(100),
    NgayNhap DATE,
    TriGia MONEY,
    TinhTrang NVARCHAR(50) DEFAULT N'Sẵn sàng', 
    FOREIGN KEY (MaTheLoai) REFERENCES THELOAISACH(MaTheLoai)
);

-- 7. PHIEUMUON
CREATE TABLE PHIEUMUON (
    MaPhieuMuon INT PRIMARY KEY IDENTITY(1,1),
    MaDocGia INT,
    NgayMuon DATE NOT NULL,
    FOREIGN KEY (MaDocGia) REFERENCES DOCGIA(MaDocGia)
);

-- 8. CHITIETPHIEUMUON: Thêm trạng thái để theo dõi từng cuốn 
CREATE TABLE CHITIETPHIEUMUON (
    MaPhieuMuon INT,
    MaSach INT,
    TrangThai NVARCHAR(50) DEFAULT N'Đang mượn', -- 'Đã trả', 'Mất'
    PRIMARY KEY (MaPhieuMuon, MaSach),
    FOREIGN KEY (MaPhieuMuon) REFERENCES PHIEUMUON(MaPhieuMuon),
    FOREIGN KEY (MaSach) REFERENCES SACH(MaSach)
);

-- 9. PHIEUTRA
CREATE TABLE PHIEUTRA (
    MaPhieuTra INT PRIMARY KEY IDENTITY(1,1),
    MaDocGia INT,
    NgayTra DATE NOT NULL,
    TienPhatKyNay MONEY DEFAULT 0,
    FOREIGN KEY (MaDocGia) REFERENCES DOCGIA(MaDocGia)
);

-- 10. CHITIETPHIEUTRA: Link trực tiếp tới Phiếu Mượn để tính ngày trễ 
CREATE TABLE CHITIETPHIEUTRA (
    MaPhieuTra INT,
    MaPhieuMuon INT, -- Link tới PM gốc
    MaSach INT,
    SoNgayMuon INT,
    TienPhat MONEY DEFAULT 0,
    PRIMARY KEY (MaPhieuTra, MaPhieuMuon, MaSach),
    FOREIGN KEY (MaPhieuTra) REFERENCES PHIEUTRA(MaPhieuTra),
    FOREIGN KEY (MaPhieuMuon, MaSach) REFERENCES CHITIETPHIEUMUON(MaPhieuMuon, MaSach)
);

-- 11. PHIEUTHUTIENPHAT
CREATE TABLE PHIEUTHUTIENPHAT (
    MaPhieuThu INT PRIMARY KEY IDENTITY(1,1),
    MaDocGia INT,
    SoTienThu MONEY NOT NULL,
    ConLai MONEY,
    NgayThu DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaDocGia) REFERENCES DOCGIA(MaDocGia)
);
