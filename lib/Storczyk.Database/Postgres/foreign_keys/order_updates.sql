CALL add_constraint('prodpol.order_updates',
                    'FK_order_updates_orders',
                    'FOREIGN KEY (order_id) ' ||
                    'REFERENCES prodpol.orders ' ||
                    'ON UPDATE CASCADE ' ||
                    'ON DELETE CASCADE', TRUE);
CALL add_constraint('prodpol.order_updates',
                    'FK_order_updates_employees',
                    'FOREIGN KEY (employee_id) ' ||
                    'REFERENCES prodpol.employees ' ||
                    'ON UPDATE CASCADE ' ||
                    'ON DELETE SET NULL', TRUE);
CALL add_constraint('prodpol.order_updates',
                    'FK_order_updates_states',
                    'FOREIGN KEY (state) ' ||
                    'REFERENCES prodpol.order_states (order_state_id) ' ||
                    'ON UPDATE CASCADE ' ||
                    'ON DELETE SET NULL', TRUE)