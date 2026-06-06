CREATE OR REPLACE FUNCTION prodpol.ordered_employees(
    _ordering_key prodpol.employee_ordering_keys,
    _asc bool) RETURNS SETOF prodpol.employees_with_roles
    LANGUAGE plpgsql
    PARALLEL SAFE
AS $$
BEGIN
    RETURN QUERY
        SELECT *
        FROM prodpol.employees_with_roles
        ORDER BY CASE
                     WHEN _asc THEN
                         CASE _ordering_key
                             WHEN 'employee_full_name' THEN (normalized_name, role_name, employee_id)
                             WHEN 'employee_email' THEN (normalized_email, role_name, employee_id)
                             WHEN 'employee_phone_number' THEN (phone_number, role_name, employee_id)
                             WHEN 'employee_role_name' THEN (role_name, employee_id)
                             ELSE (employee_id, employee_id)
                             END
                     END ASC NULLS LAST,
                 CASE
                     WHEN NOT _asc THEN
                         CASE _ordering_key
                             WHEN 'employee_full_name' THEN (normalized_name, role_name, employee_id)
                             WHEN 'employee_email' THEN (normalized_email, role_name, employee_id)
                             WHEN 'employee_phone_number' THEN (phone_number, role_name, employee_id)
                             WHEN 'employee_role_name' THEN (role_name, employee_id)
                             ELSE (employee_id, employee_id)
                             END
                     END DESC NULLS LAST;
END;
$$;