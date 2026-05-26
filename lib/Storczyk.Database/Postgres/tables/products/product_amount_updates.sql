/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CREATE TABLE IF NOT EXISTS prodpol.product_amount_updates
(
    update_id         bigserial PRIMARY KEY,
    product_id        bigint    NOT NULL,
    created_at        timestamp NOT NULL DEFAULT now(),
    amount_current    integer   NOT NULL,
    amount_delta      integer   NOT NULL,
    created_by        bigint
);