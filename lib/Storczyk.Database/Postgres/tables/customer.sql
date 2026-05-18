CREATE TABLE IF NOT EXISTS "prodpol"."customers"
(
    customer_id      bigserial    NOT NULL PRIMARY KEY,
    email            varchar(128) NOT NULL,
    normalized_email text generated always as ( lower(email) ) STORED,
    phone_number     varchar(128),
    password_hash    varchar(128),
    email_confirmed  boolean      NOT NULL DEFAULT false,
    role             integer,
    name_first       varchar(128),
    name_last        varchar(128),
    company_name     varchar(128),
    full_name        text generated always as ( name_first || ' ' || name_last ) STORED,
    normalized_name  text generated always as ( lower(name_first || ' ' || name_last) ) STORED
);