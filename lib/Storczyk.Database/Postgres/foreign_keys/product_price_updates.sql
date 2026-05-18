CALL prodpol.add_constraint(
        'prodpol.product_price_updates',
        'FK_product_price_updates_products',
        'FOREIGN KEY (product_id) REFERENCES prodpol.products (product_id)' ||
        'ON UPDATE CASCADE ON DELETE CASCADE');
CALL prodpol.add_constraint(
        'prodpol.product_price_updates',
        'FK_product_price_updates_employees',
        'FOREIGN KEY (modified_by) REFERENCES prodpol.employees (employee_id)' ||
        'ON UPDATE CASCADE ON DELETE SET NULL');