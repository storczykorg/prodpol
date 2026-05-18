/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CREATE TABLE IF NOT EXISTS prodpol.employee_roles
(
    role_id      serial       NOT NULL,
    display_name varchar(128) NOT NULL,
    role_name    varchar(64)  NOT NULL,
    PRIMARY KEY (role_id),
    CONSTRAINT UQ_employee_roles_role_name UNIQUE (role_name)
        INCLUDE (role_id)
);

