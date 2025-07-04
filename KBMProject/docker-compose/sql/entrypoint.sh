#!/bin/bash

# Start SQL Server in the background
/opt/mssql/bin/sqlservr &

# Start SQL Server in the background
/opt/mssql/bin/sqlservr &

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to start..."
sleep 15

# Use environment variables
DB_NAME=${DB_NAME:-KBMDb}
DB_USER=${DB_USER:-kbm_user}
DB_USER_PASSWORD=${DB_USER_PASSWORD:-StrongPassword123!}

# Execute SQL via piping the here-document into sqlcmd
cat <<EOF | /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -b
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '$DB_NAME')
BEGIN
    CREATE DATABASE [$DB_NAME];
END
GO

USE [$DB_NAME];
GO

IF NOT EXISTS (SELECT * FROM sys.sql_logins WHERE name = '$DB_USER')
BEGIN
    CREATE LOGIN [$DB_USER] WITH PASSWORD = '$DB_USER_PASSWORD';
END
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = '$DB_USER')
BEGIN
    CREATE USER [$DB_USER] FOR LOGIN [$DB_USER];
    ALTER ROLE db_owner ADD MEMBER [$DB_USER];
END
GO
EOF

# Wait to keep container alive
wait