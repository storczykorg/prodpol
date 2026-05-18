CALL add_constraint('prodpol.employee_photos',
                    'FK_employee_photos_employees',
                    'FOREIGN KEY (employee_id) ' ||
                    'REFERENCES prodpol.employees', TRUE);