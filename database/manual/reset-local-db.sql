-- Chỉ dùng local dev khi cần reset database.
-- Cẩn thận: script này xóa database local.

USE master;
GO

IF DB_ID('DineFlowDb') IS NOT NULL
BEGIN
    ALTER DATABASE DineFlowDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE DineFlowDb;
END
GO
