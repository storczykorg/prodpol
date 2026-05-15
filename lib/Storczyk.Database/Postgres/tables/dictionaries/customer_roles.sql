CREATE TABLE IF NOT EXISTS prodpol.customer_roles
(
    role_id      serial                NOT NULL,
    display_name varchar(128)          NOT NULL,
    role_name    character varying(64) NOT NULL,
    PRIMARY KEY (role_id),
    UNIQUE (role_name)
        INCLUDE (role_id)
);

