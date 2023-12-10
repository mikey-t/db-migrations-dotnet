-- Once DbMigrations project is bootstrapped, do the following:
--   - Run: swig dbAddMigration AddPerson
--   - Add the contents of this file to the newly generated file at ./src/DbMigrations/Scripts/AddPerson.sql
--   - Add the contents of ExampleMigration_Down.sql to the newly generated file at ./src/DbMigrations/Scripts/AddPerson_Down.sql
--   - Run: swig dbMigrate
CREATE TABLE IF NOT EXISTS public.person (
    id integer NOT NULL GENERATED ALWAYS AS IDENTITY (
        INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1
    ),
    first_name character varying(100) COLLATE pg_catalog."default",
    last_name character varying(100) COLLATE pg_catalog."default",
    CONSTRAINT person_pkey PRIMARY KEY (id)
) TABLESPACE pg_default;
ALTER TABLE IF EXISTS public.person OWNER to :DB_USER;

