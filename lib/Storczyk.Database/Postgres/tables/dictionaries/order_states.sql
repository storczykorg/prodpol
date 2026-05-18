/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CREATE TABLE IF NOT EXISTS prodpol.order_states
(
    order_state_id serial                NOT NULL,
    display_name   varchar(128)          NOT NULL,
    state_name     character varying(64) NOT NULL,
    PRIMARY KEY (order_state_id),
    UNIQUE (state_name)
        INCLUDE (order_state_id)
);


