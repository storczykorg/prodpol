CALL add_constraint('prodpol.employees', 'FK_employees_roles',
                    'foreign key (role_id) references prodpol.employee_roles (role_id)
                       on delete set null
                          on update cascade')