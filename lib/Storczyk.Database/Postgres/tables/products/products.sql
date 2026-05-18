/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CREATE TABLE IF NOT EXISTS prodpol.products
(
    product_id       bigserial                   NOT NULL,
    created_at       timestamp without time zone NOT NULL DEFAULT now(),
    created_by       bigint                      NOT NULL,
    last_modified_by bigint                      NOT NULL,
    last_modified_at timestamp without time zone NOT NULL DEFAULT now(),
    price            money                       NOT NULL,
    unit_type        integer                     NOT NULL,
    available_amount integer                     NOT NULL DEFAULT 0,
    unit_base        integer                     NOT NULL DEFAULT 1,
    PRIMARY KEY (product_id)
);