CREATE OR REPLACE FUNCTION prodpol.ordered_employees(
    _ordering_key prodpol.employee_ordering_keys,
    _asc bool) RETURNS SETOF prodpol.employees_with_roles
    LANGUAGE plpgsql
    PARALLEL SAFE
AS $$
BEGIN
    RETURN QUERY
    SELECT employee_id,
           role_id,
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
           role_display_name
    FROM prodpol.employees_with_roles
    ORDER BY
        CASE WHEN _asc THEN
            CASE _ordering_key
                WHEN 'employee_full_name' THEN normalized_name
                WHEN 'employee_email' THEN normalized_email
                WHEN 'employee_phone_number' THEN phone_number
            END
        END ASC,
        CASE WHEN NOT _asc THEN
            CASE _ordering_key
                WHEN 'employee_full_name' THEN normalized_name
                WHEN 'employee_email' THEN normalized_email
                WHEN 'employee_phone_number' THEN phone_number
            END
        END DESC,
        CASE WHEN _asc THEN employee_id END ASC,
        CASE WHEN NOT _asc THEN employee_id END DESC;
END;
$$;

CREATE OR REPLACE FUNCTION prodpol.ordered_employees(
    _ordering_key prodpol.employee_ordering_keys,
    _asc bool,
    _previous_employee bigint,
    _limit int) RETURNS SETOF prodpol.employees_with_roles
    LANGUAGE plpgsql
    PARALLEL SAFE
AS $$
DECLARE
    _p_val text;
BEGIN
    -- Get the value of the ordering column for the previous employee (for keyset pagination)
    IF _ordering_key <> 'employee_id' THEN
        SELECT CASE _ordering_key
                   WHEN 'employee_full_name' THEN normalized_name
                   WHEN 'employee_email' THEN normalized_email
                   WHEN 'employee_phone_number' THEN phone_number
               END
        INTO _p_val
        FROM prodpol.employees_with_roles
        WHERE employee_id = _previous_employee;
    END IF;

    RETURN QUERY
    SELECT employee_id,
           role_id,
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
           role_display_name
    FROM prodpol.employees_with_roles
    WHERE
        CASE WHEN _asc THEN
            CASE _ordering_key
                WHEN 'employee_id'           THEN employee_id > _previous_employee
                WHEN 'employee_full_name'    THEN (normalized_name, employee_id) > (_p_val, _previous_employee)
                WHEN 'employee_email'       THEN (normalized_email, employee_id) > (_p_val, _previous_employee)
                WHEN 'employee_phone_number' THEN (phone_number, employee_id) > (_p_val, _previous_employee)
            END
        ELSE
            CASE _ordering_key
                WHEN 'employee_id'           THEN employee_id < _previous_employee
                WHEN 'employee_full_name'    THEN (normalized_name, employee_id) < (_p_val, _previous_employee)
                WHEN 'employee_email'       THEN (normalized_email, employee_id) < (_p_val, _previous_employee)
                WHEN 'employee_phone_number' THEN (phone_number, employee_id) < (_p_val, _previous_employee)
            END
        END
    ORDER BY
        CASE WHEN _asc THEN
            CASE _ordering_key
                WHEN 'employee_full_name' THEN normalized_name
                WHEN 'employee_email' THEN normalized_email
                WHEN 'employee_phone_number' THEN phone_number
            END
        END ASC,
        CASE WHEN NOT _asc THEN
            CASE _ordering_key
                WHEN 'employee_full_name' THEN normalized_name
                WHEN 'employee_email' THEN normalized_email
                WHEN 'employee_phone_number' THEN phone_number
            END
        END DESC,
        CASE WHEN _asc THEN employee_id END ASC,
        CASE WHEN NOT _asc THEN employee_id END DESC
    LIMIT _limit;
END;
$$;