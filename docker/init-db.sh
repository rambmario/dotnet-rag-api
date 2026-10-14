#!/bin/bash
set -eu

HOST="${MSSQL_HOST:-sqlserver}"
PASSWORD="${MSSQL_SA_PASSWORD:?MSSQL_SA_PASSWORD is required}"
SQLCMD="/opt/mssql-tools18/bin/sqlcmd"

echo "Waiting for SQL Server at ${HOST}..."
for i in $(seq 1 30); do
  if ${SQLCMD} -S "${HOST}" -U sa -P "${PASSWORD}" -C -Q "SELECT 1" >/dev/null 2>&1; then
    echo "SQL Server is ready."
    break
  fi
  sleep 2
done

${SQLCMD} -S "${HOST}" -U sa -P "${PASSWORD}" -C -i /scripts/create-database.sql
echo "Database initialization completed."
