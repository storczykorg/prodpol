CALL add_constraint('prodpol.product_categories',
                    'FK_product_categories_itself',
                    'FOREIGN KEY (parent_id) ' ||
                    'REFERENCES prodpol.product_categories (category_id)', TRUE);