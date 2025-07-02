#!/bin/bash

# Start SQL Server in background
/opt/mssql/bin/sqlservr &

echo "Waiting for SQL Server to be available..."
sleep 15

# Run init script
./opt/mssql-tools/bin/sqlcmd -S sqlserver -d master -U sa -P StrongPassword123! -i /docker-entrypoint-initdb.d/init.sql

wait