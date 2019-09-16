CREATE EXTENSION IF NOT EXISTS dblink;
CREATE EXTENSION IF NOT EXISTS citext;

DO $$
BEGIN
PERFORM dblink_exec('', 'CREATE DATABASE' || :dbName);
EXCEPTION WHEN duplicate_database THEN RAISE NOTICE '%, skipping', SQLERRM USING ERRCODE = SQLSTATE;
END
$$;

DO
$do$
BEGIN
   IF NOT EXISTS (
      SELECT
      FROM   pg_catalog.pg_roles
      WHERE  rolname = :dbUser) THEN

      CREATE ROLE :dbUser LOGIN PASSWORD :dbPass;
	  GRANT ALL PRIVILEGES ON DATABASE :dbName TO :dbUser;
   END IF;
END
$do$;

ALTER ROLE :dbUser SUPERUSER;

SELECT dblink_exec('dbname=' || :dbName || ' user=' || :dbUser ' password=' || :dbPass || '', 'CREATE EXTENSION IF NOT EXISTS dblink;');
SELECT dblink_exec('dbname=' || :dbName || ' user=' || :dbUser ' password=' || :dbPass || '', 'GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public to ' || :dbUserdnsuser || ';');
SELECT dblink_exec('dbname=' || :dbName || ' user=' || :dbUser ' password=' || :dbPass || '', 'GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public to ' || :dbUserdnsuser || ';');
SELECT dblink_exec('dbname=' || :dbName || ' user=' || :dbUser ' password=' || :dbPass || '', 'GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public to ' || :dbUserdnsuser || ';');
SELECT dblink_exec('dbname=' || :dbName || ' user=' || :dbUser ' password=' || :dbPass || '', 'CREATE EXTENSION IF NOT EXISTS citext;');