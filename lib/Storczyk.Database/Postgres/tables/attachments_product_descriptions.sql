/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CREATE TABLE IF NOT EXISTS "prodpol"."attachments_product_descriptions"
(
    id                              bigserial PRIMARY KEY,
    attachments_attachment_id       bigint  NOT NULL,
    product_descriptions_product_id bigint  NOT NULL,
    type                            integer NOT NULL
);
