/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CREATE TABLE IF NOT EXISTS prodpol.delivery_methods
(
    delivery_id   serial                NOT NULL,
    base_cost     money                 NOT NULL,
    delivery_name character varying(64) NOT NULL,
    PRIMARY KEY (delivery_id),
    UNIQUE (delivery_name)
        INCLUDE (delivery_id)
);

