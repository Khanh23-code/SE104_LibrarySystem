-- Migration: Add missing columns if they do not exist
-- Run this script against your SQL Server database used by the application.
-- It will add commonly-missing columns discovered from the model files in the workspace.

-- Add AnhDaiDien to DOCGIA (avatar path)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'AnhDaiDien' AND Object_ID = Object_ID(N'DOCGIA')
)
BEGIN
    ALTER TABLE DOCGIA ADD AnhDaiDien NVARCHAR(500) NULL;
END
GO

-- Ensure SACH has TenSach (book title)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'TenSach' AND Object_ID = Object_ID(N'SACH')
)
BEGIN
    ALTER TABLE SACH ADD TenSach NVARCHAR(100) NULL;
END
GO

-- Ensure SACH has MaISBN
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'MaISBN' AND Object_ID = Object_ID(N'SACH')
)
BEGIN
    ALTER TABLE SACH ADD MaISBN VARCHAR(50) NULL;
END
GO

-- Ensure SACH has NhaXuatBan
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'NhaXuatBan' AND Object_ID = Object_ID(N'SACH')
)
BEGIN
    ALTER TABLE SACH ADD NhaXuatBan NVARCHAR(100) NULL;
END
GO

-- Ensure SACH has NamXuatBan
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'NamXuatBan' AND Object_ID = Object_ID(N'SACH')
)
BEGIN
    ALTER TABLE SACH ADD NamXuatBan INT NULL;
END
GO

-- If you need to make any column NOT NULL or add defaults, do it explicitly after
-- verifying existing data. This script avoids forcing NOT NULL to keep it safe on
-- live databases.

PRINT 'AddMissingColumns.sql completed.';

-- Ensure THAMSO contains TongNoToiDa (maximum allowed debt)
