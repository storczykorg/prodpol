--- Add `test.produkt` to categories `test`, `materials_other`, `products_other`
WITH test_category as (SELECT category_id
                       FROM prodpol.product_categories
                       WHERE category_name IN ('test', 'materials_other', 'products_other')),
     test_product as (SELECT product_id
                      FROM prodpol.products
                      WHERE product_name = 'test.produkt'
                      LIMIT 1)

INSERT
INTO prodpol.product_categories_products (categories_category_id, products_product_id)
SELECT test_category.category_id,
       test_product.product_id
FROM test_category,
     test_product
ON CONFLICT DO NOTHING;

--- Add `test.material` to categories `test`, `steel`
WITH test_category as (SELECT category_id
                       FROM prodpol.product_categories
                       WHERE category_name IN ('test', 'steel')),
     test_product as (SELECT product_id
                      FROM prodpol.products
                      WHERE product_name = 'test.material'
                      LIMIT 1)

INSERT
INTO prodpol.product_categories_products (categories_category_id, products_product_id)
SELECT test_category.category_id,
       test_product.product_id
FROM test_category,
     test_product
ON CONFLICT DO NOTHING;

--- Add `test.part` to categories `test`, `screws`
WITH test_category as (SELECT category_id
                       FROM prodpol.product_categories
                       WHERE category_name IN ('test', 'screws')),
     test_product as (SELECT product_id
                      FROM prodpol.products
                      WHERE product_name = 'test.part'
                      LIMIT 1)

INSERT
INTO prodpol.product_categories_products (categories_category_id, products_product_id)
SELECT test_category.category_id,
       test_product.product_id
FROM test_category,
     test_product
ON CONFLICT DO NOTHING;