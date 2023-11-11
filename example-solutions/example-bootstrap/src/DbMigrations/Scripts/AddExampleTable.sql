CREATE TABLE public.my_test_table
(
    id uuid NOT NULL,
    first_name character varying(150),
    last_name character varying(150),
    created_at timestamp(0) without time zone NOT NULL DEFAULT now(),
    PRIMARY KEY (id)
);

ALTER TABLE IF EXISTS public.my_test_table
    OWNER to :DB_USER;
