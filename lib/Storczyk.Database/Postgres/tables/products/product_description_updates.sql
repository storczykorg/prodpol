/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CREATE TABLE IF NOT EXISTS prodpol.product_description_updates
(
    product_id       bigint                      NOT NULL,
    update_id        bigint                      NOT NULL,
    language_code    varchar(5),
    update_at        timestamp without time zone NOT NULL DEFAULT now(),
    title            varchar(512)                NOT NULL,
    body             text                        NOT NULL,
    is_public        boolean                     NOT NULL DEFAULT false,
    PRIMARY KEY (product_id, language_code, update_id)
);