CALL add_constraint('prodpol.order_products',
                    'FK_order_products_orders',
                    'FOREIGN KEY (order_id) ' ||
                    'REFERENCES prodpol.orders (order_id) ' ||
                    'ON UPDATE CASCADE ' ||
                    'ON DELETE CASCADE', TRUE);
CALL add_constraint('prodpol.order_products',
                    'FK_order_products_products',
                    'FOREIGN KEY (product_id) ' ||
                    'REFERENCES prodpol.products (product_id) ' ||
                    'ON UPDATE CASCADE ' ||
                    'ON DELETE RESTRICT', TRUE)