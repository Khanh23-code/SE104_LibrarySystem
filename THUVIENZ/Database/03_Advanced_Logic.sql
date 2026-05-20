-- ======================================================================
-- DATABASE INFRASTRUCTURE: ADVANCED LOGIC (TRIGGERS, SPs, INDEXES)
-- Tối ưu hóa tự động cập nhật trạng thái và xuất báo cáo thư viện
-- ======================================================================
USE QL_ThuVien;
GO

-- 1. Trigger tự động cập nhật trạng thái CUONSACH khi có thay đổi mượn/trả
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_SyncCuonSachStatus')
    DROP TRIGGER trg_SyncCuonSachStatus;
GO

CREATE TRIGGER trg_SyncCuonSachStatus
ON CHITIETMUONTRA
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- A. Trường hợp Mượn sách (Insert mới hoặc Update NgayTraThucTe vẫn là NULL)
    UPDATE CS
    SET TinhTrang = N'Đang mượn'
    FROM CUONSACH CS
    INNER JOIN inserted i ON CS.MaCuonSach = i.MaCuonSach
    WHERE i.NgayTraThucTe IS NULL;

    -- B. Trường hợp Trả sách (Update NgayTraThucTe chuyển sang có giá trị)
    UPDATE CS
    SET TinhTrang = ISNULL(i.TinhTrangCuonSachKhiTra, N'Sẵn sàng')
    FROM CUONSACH CS
    INNER JOIN inserted i ON CS.MaCuonSach = i.MaCuonSach
    WHERE i.NgayTraThucTe IS NOT NULL;
END;
GO

-- 2. Stored Procedure xuất báo cáo quá hạn (Dựa trên cấu trúc gộp mới)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetOverdueReport')
    DROP PROCEDURE sp_GetOverdueReport;
GO

CREATE PROCEDURE sp_GetOverdueReport
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @fineRate MONEY;

    -- Lấy mức phạt quy định từ bảng THAMSO
    SELECT @fineRate = CAST(GiaTri AS MONEY) FROM THAMSO WHERE TenThamSo = 'TienPhatMoiNgay';
    SET @fineRate = ISNULL(@fineRate, 2000);

    -- Truy vấn danh sách các cuốn sách chưa trả và đã vượt quá HanTra
    SELECT 
        DG.HoTen AS [TenDocGia],
        DG.SoDienThoai AS [SoDienThoai],
        S.TenSach AS [TenSach],
        CS.MaCuonSach AS [MaCuonSach],
        PM.NgayMuon AS [NgayMuon],
        CT.HanTra AS [HanTra],
        DATEDIFF(day, CT.HanTra, GETDATE()) AS [SoNgayTre],
        (DATEDIFF(day, CT.HanTra, GETDATE()) * @fineRate) AS [TienPhatDuKien]
    FROM CHITIETMUONTRA CT
    JOIN PHIEUMUON PM ON CT.MaPhieuMuon = PM.MaPhieuMuon
    JOIN DOCGIA DG ON PM.MaDocGia = DG.MaDocGia
    JOIN CUONSACH CS ON CT.MaCuonSach = CS.MaCuonSach
    JOIN SACH S ON CS.MaSach = S.MaSach
    WHERE CT.NgayTraThucTe IS NULL
      AND CT.HanTra < GETDATE();
END;
GO

-- 3. Tối ưu hóa hiệu năng tìm kiếm (Performance Indexing)
IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'idx_Sach_MaISBN')
    CREATE NONCLUSTERED INDEX idx_Sach_MaISBN ON SACH (MaISBN);

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'idx_Sach_TenSach')
    CREATE NONCLUSTERED INDEX idx_Sach_TenSach ON SACH (TenSach);

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'idx_DocGia_HoTen_SĐT')
    CREATE NONCLUSTERED INDEX idx_DocGia_HoTen_SĐT ON DOCGIA (HoTen, SoDienThoai);
GO
