CREATE TABLE IF NOT EXISTS prodpol.product_attachment_types
(
    type_id      serial                 NOT NULL,
    type_name    character varying(128) NOT NULL,
    display_name character varying(128) NOT NULL,
    PRIMARY KEY (type_id),
    UNIQUE (type_name)
        INCLUDE (type_id)
);

