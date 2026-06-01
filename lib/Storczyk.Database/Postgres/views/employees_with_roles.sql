CREATE OR REPLACE VIEW prodpol.employees_with_roles
AS
(
SELECT employee_id,
       employees.role_id,
       email,
       normalized_email,
       name_first,
       name_last,
       full_name,
       normalized_name,
       phone_number,
       password_hash,
       created_at,
       enabled,
       role_name,
       er.display_name as role_display_name
FROM prodpol.employees
         LEFT JOIN prodpol.employee_roles er on employees.role_id = er.role_id
    )