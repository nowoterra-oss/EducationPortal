-- SQL Server Bağlantı ve Database Testi
-- SSMS'de bu script'i çalıştırın

-- 1. SQL Server versiyonunu kontrol et
SELECT @@VERSION AS 'SQL Server Version';
GO

-- 2. SQL Server instance adını kontrol et
SELECT @@SERVERNAME AS 'Server Name';
GO

-- 3. Mevcut database'leri listele
SELECT name AS 'Database Name',
       create_date AS 'Created Date',
       state_desc AS 'State'
FROM sys.databases
ORDER BY name;
GO

-- 4. EduPortalDb var mı kontrol et
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'EduPortalDb')
BEGIN
    PRINT '✅ EduPortalDb database mevcut!'

    USE EduPortalDb;
    GO

    -- Tabloları say
    SELECT COUNT(*) AS 'Total Tables'
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_TYPE = 'BASE TABLE';

    -- Tabloları listele
    SELECT TABLE_NAME AS 'Table Name'
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_TYPE = 'BASE TABLE'
    ORDER BY TABLE_NAME;

    -- Rolleri kontrol et
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetRoles')
    BEGIN
        SELECT COUNT(*) AS 'Total Roles' FROM AspNetRoles;
        SELECT * FROM AspNetRoles;
    END

    -- Admin kullanıcıyı kontrol et
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers')
    BEGIN
        SELECT COUNT(*) AS 'Total Users' FROM AspNetUsers;
        SELECT Email, UserName, FirstName, LastName, IsActive
        FROM AspNetUsers
        WHERE Email = 'admin@eduportal.com';
    END
END
ELSE
BEGIN
    PRINT '❌ EduPortalDb database bulunamadı!'
    PRINT 'Database oluşturulması gerekiyor.'

    -- Database'i oluştur
    PRINT 'Database oluşturuluyor...'
    CREATE DATABASE EduPortalDb;
    PRINT '✅ EduPortalDb database oluşturuldu!'
    PRINT 'Şimdi migration çalıştırılmalı: dotnet ef database update'
END
GO

-- 5. SQL Server servislerini kontrol et
SELECT servicename, status_desc, startup_type_desc, last_startup_time
FROM sys.dm_server_services;
GO

-- 6. Bağlantı bilgilerini göster
SELECT
    'Server Name' AS 'Property', @@SERVERNAME AS 'Value'
UNION ALL
SELECT
    'SQL Server Version', @@VERSION
UNION ALL
SELECT
    'Current Database', DB_NAME()
UNION ALL
SELECT
    'Current User', SYSTEM_USER
UNION ALL
SELECT
    'Connection ID', CAST(@@SPID AS VARCHAR(10));
GO
