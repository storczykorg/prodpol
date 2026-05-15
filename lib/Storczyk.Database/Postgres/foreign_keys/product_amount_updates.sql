CALL add_constraint('prodpol.product_amount_updates',
     'FK_product_amount_updates_products',
     'FOREIGN KEY (product_id) ' ||
     'REFERENCES prodpol.products',
     TRUE);
CALL add_constraint('prodpol.product_amount_updates',
                    'FK_product_amount_updates_employees',
                    'FOREIGN KEY (created_by) ' ||
                    'REFERENCES prodpol.employees (employee_id)',
                    TRUE);
CALL add_constraint('prodpol.product_amount_updates',
                    'FK_product_amount_updates_order_updates',
                    'FOREIGN KEY (related_update_id) ' ||
                    'REFERENCES prodpol.order_updates (update_id)',
                    TRUE);