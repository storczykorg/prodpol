/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CREATE TABLE IF NOT EXISTS "prodpol"."attachments"
(
    attachment_id bigserial                   NOT NULL,
    uploaded_by   bigint                      NOT NULL,
    modified_by   bigint                      NOT NULL,
    uploaded_at   timestamp without time zone NOT NULL DEFAULT now(),
    modified_at   timestamp without time zone NOT NULL DEFAULT now(),
    mime_type     character varying(128)      NOT NULL,
    alt_text      varchar(128),
    screen_size   point,
    type          integer                     NOT NULL,
    payload       varchar(128)                NOT NULL,
    PRIMARY KEY (attachment_id)
);
