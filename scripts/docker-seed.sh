#!/usr/bin/env bash
set -euo pipefail

SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
SQL_SERVER="${SQL_SERVER:-sqlserver}"
SQL_PORT="${SQL_PORT:-1433}"
SQL_DATABASE="${SQL_DATABASE:-DynamicHr}"

sqlcmd=(
  "$SQLCMD"
  -C
  -S "${SQL_SERVER},${SQL_PORT}"
  -U sa
  -P "$MSSQL_SA_PASSWORD"
  -d "$SQL_DATABASE"
  -b
  -r 1
)

echo "Waiting for EF Core migrations to create the EmployeeType table..."
until "${sqlcmd[@]}" -Q "IF OBJECT_ID(N'dbo.EmployeeType', N'U') IS NULL THROW 51000, 'EmployeeType table is not ready.', 1;"; do
  sleep 2
done

has_employee_types=$("${sqlcmd[@]}" -h -1 -W -Q "SET NOCOUNT ON; SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.EmployeeType) THEN 1 ELSE 0 END;" | tr -d '\r\n ')

if [[ "$has_employee_types" == "1" ]]; then
  echo "Database already contains employee types; skipping demo seed data."
  exit 0
fi

echo "Database is empty; loading professional demo seed data..."
"${sqlcmd[@]}" -i /seed-scripts/seed-professional-employee-types.sql >/dev/null
"${sqlcmd[@]}" -i /seed-scripts/seed-professional-employees.sql >/dev/null
echo "Demo seed data loaded successfully."
