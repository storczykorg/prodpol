CALL add_constraint('prodpol.product_categories_products',
                    'FK_product_categories_products_products',
                    'FOREIGN KEY (categories_category_id) ' ||
                    'REFERENCES prodpol.product_categories (category_id)',
                    TRUE);
CALL add_constraint('prodpol.product_categories_products',
                    'FK_product_categories_products_product_descriptions',
                    'FOREIGN KEY (products_product_id) ' ||
                    'REFERENCES prodpol.product_descriptions (product_id)',
                    TRUE)