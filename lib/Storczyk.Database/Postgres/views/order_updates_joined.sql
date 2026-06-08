CREATE OR REPLACE VIEW order_updates_joined
AS
SELECT ou.*,
       os.state_name,
       os.display_name     as state_display_name,
       e.full_name         as employee_name,
       e.role_name         as employee_role_name,
       e.role_display_name as employee_role_display_name
FROM prodpol.order_updates ou
         LEFT JOIN prodpol.order_states os ON ou.state = os.order_state_id
         LEFT JOIN prodpol.employees_with_roles e ON e.employee_id = ou.employee_id;