CREATE OR REPLACE VIEW prodpol.employees_with_roles
AS
(
SELECT employees.*,
       role_name,
       er.display_name as role_display_name
FROM prodpol.employees
         LEFT JOIN prodpol.employee_roles er on employees.role_id = er.role_id
    )