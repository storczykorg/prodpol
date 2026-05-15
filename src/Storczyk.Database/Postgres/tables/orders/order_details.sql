CREATE TABLE IF NOT EXISTS prodpol.order_details
(
    order_id     bigint                 NOT NULL,
    first_name   character varying(256)
        constraint CK_order_details_first_name check (first_name ~ '^(\w+\s?)+$'),
    last_name    character varying(256)
        constraint CK_order_details_last_name check (first_name ~ '^(\w+\s?)+$'),
    company_name character varying(512),
    "NIP_code"   character varying(128),
    city         character varying(256) NOT NULL,
    street       character varying(256),
    zip_code     character varying(128) NOT NULL check ( zip_code ~ '^\d{2}-\d{3}$' ),
    street_no    character varying(64)  NOT NULL
        constraint CK_order_details_street_no check ( street_no ~ '^(\d+)(\w*)$' ),
    flat_no      character varying(64)
        constraint CK_order_details_flat_no check ( street_no ~ '^(\d+)(\w*)$' ),
    special_info character varying(1024),
    phone_number character varying(16)
        constraint CK_order_details_phone_number check ( phone_number ~ '^\+?[1-9][0-9]{7,14}$' ),
    PRIMARY KEY (order_id)
);

