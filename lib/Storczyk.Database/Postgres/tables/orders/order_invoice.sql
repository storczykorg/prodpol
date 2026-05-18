/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CREATE TABLE IF NOT EXISTS prodpol.order_invoice
(
    invoice_id  bigserial                   NOT NULL,
    order_id    bigint,
    customer_id bigint                      NOT NULL,
    total_cost  money,
    created_at  timestamp without time zone NOT NULL DEFAULT now(),
    e_invoice   xml,
    payload     bytea                       NOT NULL,
    PRIMARY KEY (invoice_id)
);