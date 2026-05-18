/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

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

