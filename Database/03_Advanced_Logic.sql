-- =========================================================
-- DATABASE INFRASTRUCTURE UPGRADE: ADVANCED LOGIC
-- PURPOSE: Optimization, Automation, and Advanced Reporting
-- =========================================================
USE QL_ThuVien;
GO

-- 1. Trigger tự động cập nhật trạng thái sách khi mượn (Automation)
-- Giúp đảm bảo tính nhất quán dữ liệu ngay cả khi Backend gặp lỗi.
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_AutoUpdateBookStatus')
    DROP TRIGGER trg_AutoUpdateBookStatus;
GO

CREATE TRIGGER trg_AutoUpdateBookStatus
ON CHITIETPHIEUMUON
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE SACH
    SET TinhTrang = N'Đang mượn'
    FROM SACH
    INNER JOIN inserted i ON SACH.MaSach = i.MaSach;
END;
GO

-- 2. Stored Procedure xuất báo cáo trễ hạn (Reporting)
-- Tính toán dữ liệu trễ hạn theo thời gian thực dựa trên tham số hệ thống.
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetOverdueReport')
    DROP PROCEDURE sp_GetOverdueReport;
GO

CREATE PROCEDURE sp_GetOverdueReport
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @maxDays INT;
    DECLARE @fineRate MONEY;

    -- Lấy tham số quy định từ bảng THAMSO
    SELECT @maxDays = CAST(GiaTri AS INT) FROM THAMSO WHERE TenThamSo = 'SoNgayMuonToiDa';
    SELECT @fineRate = CAST(GiaTri AS MONEY) FROM THAMSO WHERE TenThamSo = 'TienPhatMoiNgay';

    -- Thiết lập giá trị mặc định nếu tham số bị thiếu
    SET @maxDays = ISNULL(@maxDays, 7);
    SET @fineRate = ISNULL(@fineRate, 1000);

    -- Truy vấn danh sách độc giả đang quá hạn
    SELECT 
        DG.HoTen AS [TenDocGia],
        S.TenSach AS [TenSach],
        PM.NgayMuon AS [NgayMuon],
        DATEADD(day, @maxDays, PM.NgayMuon) AS [HanTra],
        DATEDIFF(day, DATEADD(day, @maxDays, PM.NgayMuon), GETDATE()) AS [SoNgayTre],
        (DATEDIFF(day, DATEADD(day, @maxDays, PM.NgayMuon), GETDATE()) * @fineRate) AS [TienPhatDuKien]
    FROM DOCGIA DG
    JOIN PHIEUMUON PM ON DG.MaDocGia = PM.MaDocGia
    JOIN CHITIETPHIEUMUON CT ON PM.MaPhieuMuon = CT.MaPhieuMuon
    JOIN SACH S ON CT.MaSach = S.MaSach
    WHERE CT.TrangThai = N'Đang mượn'
      AND DATEADD(day, @maxDays, PM.NgayMuon) < GETDATE();
END;
GO

-- 3. Tối ưu hóa hiệu năng tìm kiếm (Performance Indexing)
-- Thêm Non-Clustered Indexes để đảm bảo tốc độ phản hồi < 2 giây cho các cột chính.
IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'idx_Sach_TenSach')
    CREATE NONCLUSTERED INDEX idx_Sach_TenSach ON SACH (TenSach);

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'idx_DocGia_HoTen')
    CREATE NONCLUSTERED INDEX idx_DocGia_HoTen ON DOCGIA (HoTen);
GO
