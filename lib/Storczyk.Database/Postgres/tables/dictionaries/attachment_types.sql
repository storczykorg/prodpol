/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CREATE TABLE IF NOT EXISTS prodpol.attachment_types
(
    type_id      serial       NOT NULL,
    type_name    varchar(128) NOT NULL unique,
    display_name varchar(128) NOT NULL,
    PRIMARY KEY (type_id)
);

