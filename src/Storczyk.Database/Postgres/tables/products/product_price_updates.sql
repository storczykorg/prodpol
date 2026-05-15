CREATE TABLE IF NOT EXISTS prodpol.product_price_updates
(
    price_update_id bigserial NOT NULL PRIMARY KEY,
    product_id      bigint    NOT NULL,
    price           money     NOT NULL,
    modified_by     bigint    NOT NULL,
    modified_at     timestamp NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS UX_product_price_updates_search
    ON prodpol.product_price_updates USING btree (product_id, modified_at, product_id);