/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CREATE TABLE IF NOT EXISTS prodpol.product_attachment_types
(
    type_id      serial                 NOT NULL,
    type_name    character varying(128) NOT NULL,
    display_name character varying(128) NOT NULL,
    PRIMARY KEY (type_id),
    UNIQUE (type_name)
        INCLUDE (type_id)
);

