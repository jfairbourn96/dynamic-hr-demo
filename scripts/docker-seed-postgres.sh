#!/bin/sh
set -eu

host="${POSTGRES_HOST:-postgres}"
database="${POSTGRES_DB:-DynamicHr}"
user="${POSTGRES_USER:-postgres}"

psql_command="psql -v ON_ERROR_STOP=1 -h $host -U $user -d $database"

echo "Waiting for EF Core migrations to create the EmployeeType table..."
until $psql_command -tAc 'SELECT to_regclass('"'"'public."EmployeeType"'"'"') IS NOT NULL' | grep -q t; do
  sleep 2
done

if [ "$($psql_command -tAc 'SELECT EXISTS (SELECT 1 FROM "EmployeeType")')" = "t" ]; then
  echo "Database already contains employee types; skipping demo seed data."
  exit 0
fi

echo "Database is empty; loading PostgreSQL demo seed data..."
$psql_command -f /seed-scripts/seed-professional-postgres.sql >/dev/null
echo "PostgreSQL demo seed data loaded successfully."
