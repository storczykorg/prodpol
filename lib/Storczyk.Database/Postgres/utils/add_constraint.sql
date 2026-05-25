/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

-- Skrypt pozwalający dodać ograniczenie do bazy danych oraz opcjonalnie je nadpisać
-- Source - https://stackoverflow.com/a/67799215
-- Posted by Pedro Ballesteros
-- Retrieved 2026-04-27, License - CC BY-SA 4.0


CREATE OR REPLACE PROCEDURE prodpol.add_constraint(t_name text,
                                                   c_name text,
                                                   constraint_sql text)
AS
$$
DECLARE
    cmd text := 'ALTER TABLE ' || t_name || ' ADD CONSTRAINT ' || c_name || ' ' || constraint_sql || ';';
BEGIN
    IF NOT EXISTS (SELECT constraint_schema
                        , constraint_name
                   FROM information_schema.table_constraints
                   where lower(constraint_name) = lower(c_name))
    THEN
        EXECUTE cmd;
    END IF;
END;
$$
    LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE prodpol.add_constraint(t_name text,
                                                   c_name text,
                                                   constraint_sql text,
                                                   force boolean)
AS
$$
DECLARE
    cmd text := 'ALTER TABLE ' || t_name || ' ADD CONSTRAINT ' || c_name || ' ' || constraint_sql || ';';
BEGIN
    IF force OR (NOT EXISTS (SELECT constraint_schema
                                  , constraint_name
                             FROM information_schema.table_constraints
                             where lower(constraint_name) = lower(c_name)))
    THEN
        IF force then
            Execute 'ALTER TABLE ' || t_name || ' DROP CONSTRAINT IF EXISTS ' || c_name || ';';
        END IF;
        EXECUTE cmd;
    END IF;
END;
$$
    LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE add_constraint(t_name text,
                                                   c_name text,
                                                   constraint_sql text)
AS
$$
DECLARE
    cmd text := 'ALTER TABLE ' || t_name || ' ADD CONSTRAINT ' || c_name || ' ' || constraint_sql || ';';
BEGIN
    IF NOT EXISTS (SELECT constraint_schema
                        , constraint_name
                   FROM information_schema.table_constraints
                   where lower(constraint_name) = lower(c_name))
    THEN
        EXECUTE cmd;
    END IF;
END;
$$
    LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE add_constraint(t_name text,
                                                   c_name text,
                                                   constraint_sql text,
                                                   force boolean)
AS
$$
DECLARE
    cmd text := 'ALTER TABLE ' || t_name || ' ADD CONSTRAINT ' || c_name || ' ' || constraint_sql || ';';
BEGIN
    IF force OR (NOT EXISTS (SELECT constraint_schema
                                  , constraint_name
                             FROM information_schema.table_constraints
                             where lower(constraint_name) = lower(c_name)))
    THEN
        IF force then
            Execute 'ALTER TABLE ' || t_name || ' DROP CONSTRAINT IF EXISTS ' || c_name || ';';
        END IF;
        EXECUTE cmd;
    END IF;
END;
$$
    LANGUAGE plpgsql;