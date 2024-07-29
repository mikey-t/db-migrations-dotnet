CREATE TABLE dbo.person (
    id INT IDENTITY(1, 1) NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    CONSTRAINT person_pkey PRIMARY KEY (id)
);
