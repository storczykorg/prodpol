CREATE TABLE IF NOT EXISTS "prodpol"."attachments_product_descriptions"
(
    id                              bigserial PRIMARY KEY,
    attachments_attachment_id       bigint  NOT NULL,
    product_descriptions_product_id bigint  NOT NULL,
    type                            integer NOT NULL
);
