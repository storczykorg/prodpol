CREATE OR REPLACE VIEW prodpol.customers_with_roles
AS
    SELECT customer_id,
           email,
           normalized_email,
           phone_number,
           password_hash,
           email_confirmed,
           role,
           name_first,
           name_last,
           company_name,
           full_name,
           normalized_name,
           role_id,
           display_name,
           role_name
    FROM prodpol.customers
    LEFT JOIN prodpol.customer_roles cr on cr.role_id = customers.role;