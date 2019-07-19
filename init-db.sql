CREATE EXTENSION IF NOT EXISTS dblink;
CREATE EXTENSION IF NOT EXISTS citext;

DO $$
BEGIN
PERFORM dblink_exec('', 'CREATE DATABASE dnsdb');
EXCEPTION WHEN duplicate_database THEN RAISE NOTICE '%, skipping', SQLERRM USING ERRCODE = SQLSTATE;
END
$$;

DO
$do$
BEGIN
   IF NOT EXISTS (
      SELECT
      FROM   pg_catalog.pg_roles
      WHERE  rolname = 'dnsuser') THEN

      CREATE ROLE dnsuser LOGIN PASSWORD 'dnsuser';
	  GRANT ALL PRIVILEGES ON DATABASE dnsdb TO dnsuser;
   END IF;
END
$do$;

ALTER ROLE dnsuser SUPERUSER;

SELECT dblink_exec('dbname=dnsdb user=dnsuser password=dnsuser', 'CREATE EXTENSION IF NOT EXISTS dblink;');
SELECT dblink_exec('dbname=dnsdb user=dnsuser password=dnsuser', 'GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public to dnsuser;');
SELECT dblink_exec('dbname=dnsdb user=dnsuser password=dnsuser', 'GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public to dnsuser;');
SELECT dblink_exec('dbname=dnsdb user=dnsuser password=dnsuser', 'GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public to dnsuser;');
SELECT dblink_exec('dbname=dnsdb user=dnsuser password=dnsuser', 'CREATE EXTENSION IF NOT EXISTS citext;');