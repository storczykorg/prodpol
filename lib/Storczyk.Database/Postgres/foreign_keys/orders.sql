CALL add_constraint('prodpol.orders', 'FK_orders_delivery_methods',
                    'foreign key (delivery_method)
      references prodpol.delivery_methods (delivery_id)
      on delete SET NULL ON UPDATE CASCADE', TRUE);

CALL add_constraint('prodpol.orders', 'FK_orders_order_states',
                    'foreign key (current_state)
      references prodpol.order_states (order_state_id)
      on delete set null on update cascade', TRUE);

CALL add_constraint('prodpol.orders', 'FK_orders_customers',
                    'foreign key (customer_id)' ||
                    'references prodpol.customers (customer_id)' ||
                    'on delete restrict on update cascade', TRUE)