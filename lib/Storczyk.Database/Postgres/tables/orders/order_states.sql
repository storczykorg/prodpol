CREATE TABLE IF NOT EXISTS prodpol.order_products
(
    "order_product_Id" bigserial NOT NULL,
    order_id           bigint    NOT NULL,
    total_cost         money     NOT NULL,
    amount             integer   NOT NULL,
    cost               money     NOT NULL,
    product_id         bigint    NOT NULL,
    customer_notes     varchar(128),
    PRIMARY KEY ("order_product_Id")
);

