CREATE TABLE IF NOT EXISTS prodpol.product_descriptions
(
    product_id       bigint                      NOT NULL,
    created_at       timestamp without time zone NOT NULL DEFAULT now(),
    body             text                        NOT NULL,
    is_public        boolean                     NOT NULL DEFAULT false,
    last_modified_at timestamp without time zone NOT NULL DEFAULT now(),
    PRIMARY KEY (product_id),
    constraint CK_product_descriptions_last_modified_at
        check ( last_modified_at >= created_at )
);