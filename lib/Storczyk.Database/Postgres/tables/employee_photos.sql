/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

create table if not exists "prodpol"."employee_photos"
(
    employee_id  bigint                 not null,
    mime_type    character varying(128) not null,
    payload      BYTEA                  not null,
    payload_size integer                not null,
    primary key (employee_id)
);

