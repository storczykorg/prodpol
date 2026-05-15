CALL add_constraint('prodpol.order_details', 'FK_order_details_orders',
                    'foreign key (order_id)' ||
                    'references prodpol.orders (order_id)' ||
                    'on delete restrict on update cascade', TRUE)