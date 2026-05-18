CALL add_constraint('prodpol.attachments',
                    'FK_attachments_uploaded_by_employees',
                    'FOREIGN KEY (uploaded_by) ' ||
                    'REFERENCES prodpol.employees (employee_id)', TRUE);
CALL add_constraint('prodpol.attachments',
                    'FK_attachments_modified_by_employees',
                    'FOREIGN KEY (modified_by) ' ||
                    'REFERENCES prodpol.employees (employee_id)', TRUE);
CALL add_constraint('prodpol.attachments',
                    'FK_attachments_type',
                    'FOREIGN KEY (modified_by) ' ||
                    'REFERENCES prodpol.employees (employee_id)', TRUE);
CALL add_constraint('prodpol.attachments',
                    'FK_attachments_type',
                    'FOREIGN KEY (modified_by) ' ||
                    'REFERENCES prodpol.attachment_types (type_id)' ||
                    'ON DELETE SET NULL ' ||
                    'ON UPDATE CASCADE', TRUE);