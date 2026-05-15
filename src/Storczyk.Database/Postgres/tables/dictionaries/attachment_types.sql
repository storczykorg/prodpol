CREATE TABLE IF NOT EXISTS prodpol.attachment_types
(
    type_id      serial       NOT NULL,
    type_name    varchar(128) NOT NULL unique,
    display_name varchar(128) NOT NULL,
    PRIMARY KEY (type_id)
);

