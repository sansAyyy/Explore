#!/bin/sh
set -eu

create_db() {
  db_name="$1"

  if [ -z "$db_name" ]; then
    return 0
  fi

  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    SELECT 'CREATE DATABASE "$db_name"'
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '$db_name')\gexec
EOSQL
}

create_db "${ADMIN_IDENTITY_DB:-}"
create_db "${CUSTOMER_ACCOUNT_DB:-}"
create_db "${MESSAGE_CENTER_DB:-}"
