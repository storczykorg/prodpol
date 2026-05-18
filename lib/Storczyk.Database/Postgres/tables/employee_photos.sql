/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CREATE TABLE IF NOT EXISTS "prodpol"."employee_photos"
(
    employee_id bigint                 NOT NULL,
    mime_type   character varying(128) NOT NULL,
    payload     TEXT                   NOT NULL,
    PRIMARY KEY (employee_id)
);

