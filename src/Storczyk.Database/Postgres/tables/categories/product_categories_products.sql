CREATE TABLE IF NOT EXISTS prodpol.product_categories_products
(
    id                     bigserial NOT NULL PRIMARY KEY,
    categories_category_id bigint    NOT NULL,
    products_product_id    bigint    NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS UX_product_categories_products
    ON prodpol.product_categories_products
        (categories_category_id, products_product_id)
    INCLUDE (id);