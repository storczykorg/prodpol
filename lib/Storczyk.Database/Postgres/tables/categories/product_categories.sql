/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CREATE TABLE IF NOT EXISTS prodpol.product_categories
(
    category_id   serial       NOT NULL,
    parent_id     integer,
    display_name  varchar(128) NOT NULL,
    category_name varchar(128) NOT NULL,
    category_path character varying[],
    PRIMARY KEY (category_id),
    UNIQUE (category_name)
        INCLUDE (display_name)
);

