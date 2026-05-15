CREATE TABLE IF NOT EXISTS "prodpol"."employee_photos"
(
    employee_id bigint                 NOT NULL,
    mime_type   character varying(128) NOT NULL,
    payload     TEXT                   NOT NULL,
    PRIMARY KEY (employee_id)
);

