CREATE TABLE IF NOT EXISTS prodpol.delivery_methods
(
    delivery_id   serial                NOT NULL,
    base_cost     money                 NOT NULL,
    delivery_name character varying(64) NOT NULL,
    PRIMARY KEY (delivery_id),
    UNIQUE (delivery_name)
        INCLUDE (delivery_id)
);

