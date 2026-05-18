/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CREATE TABLE IF NOT EXISTS prodpol.orders
(
    order_id        bigserial                   NOT NULL,
    customer_id     bigint                      NOT NULL,
    created_at      timestamp without time zone NOT NULL DEFAULT now(),
    employee_id     bigint                      NOT NULL,
    delivery_method integer                     NOT NULL,
    total           money                       NOT NULL,
    current_state   integer                     NOT NULL,
    PRIMARY KEY (order_id)
);