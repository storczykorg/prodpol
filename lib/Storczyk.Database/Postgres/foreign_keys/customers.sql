CALL prodpol.add_constraint('prodpol.customers',
     'FK_customers_customer_roles',
     'FOREIGN KEY (role) ' ||
     'REFERENCES prodpol.customer_roles (role_id) ' ||
     'ON UPDATE CASCADE', TRUE)