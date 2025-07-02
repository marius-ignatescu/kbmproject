USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'KBMDb')
BEGIN
    CREATE DATABASE KBMDb;
END
GO

USE KBMDb;
GO

IF NOT EXISTS (SELECT * FROM sys.sql_logins WHERE name = 'kbm_user')
BEGIN
    CREATE LOGIN kbm_user WITH PASSWORD = 'StrongPassword123!';
END
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'kbm_user')
BEGIN
    CREATE USER kbm_user FOR LOGIN kbm_user;
    ALTER ROLE db_owner ADD MEMBER kbm_user;
END
GO