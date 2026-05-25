/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CREATE TABLE IF NOT EXISTS prodpol.product_information_snapshots
(
    snapshot_id         bigserial                   NOT NULL,
    product_id          bigint                      NOT NULL,
    created             jsonb                       NOT NULL,
    modified            jsonb                       NOT NULL,
    price               money                       NOT NULL,
    unit_type           integer                     NOT NULL,
    available_amount    integer                     NOT NULL,
    unit_base           integer                     NOT NULL,
    product_name        varchar(255)                NOT NULL,
    snapshot_at         timestamp without time zone NOT NULL DEFAULT now(),
    price_updates       jsonb,
    amount_updates      jsonb,
    description_updates jsonb,
    descriptions        jsonb,
    PRIMARY KEY (snapshot_id)
);
