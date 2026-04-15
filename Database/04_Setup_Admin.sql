-- =========================================================
-- SYSTEM SETUP: DEFAULT ADMIN ACCOUNT & SCHEMA FIX
-- =========================================================
USE QL_ThuVien;
GO

-- 1. Đảm bảo cột TrangThai tồn tại trong bảng TAIKHOAN
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID('TAIKHOAN') AND name = 'TrangThai'
)
BEGIN
    ALTER TABLE TAIKHOAN ADD TrangThai NVARCHAR(20) DEFAULT 'Pending';
END
GO

-- 2. Tạo tài khoản Admin mặc định (admin / admin)
-- Nếu đã tồn tại thì cập nhật lại mật khẩu và trạng thái
IF NOT EXISTS (SELECT * FROM TAIKHOAN WHERE TenDangNhap = 'admin')
BEGIN
    INSERT INTO TAIKHOAN (TenDangNhap, MatKhau, Quyen, TrangThai)
    VALUES ('admin', 'admin', 'Admin', 'Active');
END
ELSE
BEGIN
    UPDATE TAIKHOAN 
    SET MatKhau = 'admin', Quyen = 'Admin', TrangThai = 'Active'
    WHERE TenDangNhap = 'admin';
END
GO

-- 3. Cập nhật tất cả các tài khoản hiện có sang Active (để test)
UPDATE TAIKHOAN SET TrangThai = 'Active' WHERE TrangThai IS NULL OR TrangThai = 'Pending';
GO

PRINT 'Thiet lap tai khoan admin (admin/admin) thanh cong.';
