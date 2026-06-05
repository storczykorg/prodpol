CREATE OR REPLACE FUNCTION prodpol.ordered_employees(
    _ordering_key prodpol.employee_ordering_keys,
    _asc boolean = TRUE,
    _cursor_id bigint = null,
    _limit int = 20,
    _fullname varchar default null,
    _email varchar default null,
    _phone_number varchar default null,
    _role_names varchar array default null) RETURNS SETOF prodpol.employees_with_roles
    LANGUAGE plpgsql
    PARALLEL SAFE
AS
$$
BEGIN
    RETURN QUERY
        WITH filtered_employees as (SELECT *
                                    FROM
                                        prodpol.filtered_employees(
                                                _fullname::varchar,
                                                _email::varchar,
                                                _phone_number::varchar,
                                                _role_names::varchar array,
                                                _cursor_id::bigint
                                        ))
        SELECT *
        FROM filtered_employees
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
                     END DESC NULLS LAST

        LIMIT _limit;
END;
$$;