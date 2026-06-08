create index if not exists idx_employees_employee_role
on prodpol.employees using btree (role_id, employee_id);