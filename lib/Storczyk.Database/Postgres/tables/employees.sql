CREATE TABLE IF NOT EXISTS "prodpol"."employees"
(
    employee_id      bigserial                   NOT NULL,
    role_id          integer,
    email            varchar(128)                NOT NULL,
    normalized_email text GENERATED ALWAYS AS ( lower(email) ) STORED,
    name_first       varchar(128)                NOT NULL,
    name_last        varchar(128)                NOT NULL,
    full_name        text GENERATED ALWAYS AS (name_first || ' ' || name_last) STORED,
    normalized_name  text GENERATED ALWAYS AS (lower(name_first || ' ' || name_last)) STORED,
    phone_number     varchar(128)                NOT NULL,
    password_hash    varchar(128), --.NET wymusza nullable hashe haseł do uwierzytelniania
    created_at       timestamp without time zone NOT NULL DEFAULT NOW(),
    enabled          boolean                     NOT NULL DEFAULT false,
    PRIMARY KEY (employee_id),
    CONSTRAINT UQ_employees_email UNIQUE (normalized_email),
    CONSTRAINT CK_format_employees_name_first check ( name_first ~ '^(\w+\s?)+$' ),
    CONSTRAINT CK_format_employees_name_last check ( name_last ~ '^(\w+\s?)+$' )
);

CALL add_constraint('prodpol.employees',
                    'CK_format_employees_email',
                    'CHECK (prodpol.valid_email(email))'::text, TRUE);
CALL add_constraint('prodpol.employees',
                    'CK_format_employees_phone_number',
                    'CHECK (prodpol.valid_phone_number(phone_number))',
                    TRUE);