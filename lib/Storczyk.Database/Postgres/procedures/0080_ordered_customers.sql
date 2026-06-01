CREATE OR REPLACE FUNCTION prodpol.ordered_customers(
    _ordering_key prodpol.customer_ordering_keys,
    _asc bool) RETURNS SETOF prodpol.customers
    LANGUAGE plpgsql
    PARALLEL SAFE
AS $$
BEGIN
    RETURN QUERY
        SELECT *
        FROM prodpol.customers
        ORDER BY
            CASE WHEN _asc THEN
                     CASE _ordering_key
                         WHEN 'customer_full_name' THEN normalized_name
                         WHEN 'customer_email' THEN normalized_email
                         WHEN 'customer_phone_number' THEN phone_number
                         END
                END ASC,
            CASE WHEN NOT _asc THEN
                     CASE _ordering_key
                         WHEN 'customer_full_name' THEN normalized_name
                         WHEN 'customer_email' THEN normalized_email
                         WHEN 'customer_phone_number' THEN phone_number
                         END
                END DESC,
            CASE WHEN _asc THEN customer_id END ASC,
            CASE WHEN NOT _asc THEN customer_id END DESC;
END;
$$;

CREATE OR REPLACE FUNCTION prodpol.ordered_customers(
    _ordering_key prodpol.customer_ordering_keys,
    _asc bool,
    _previous_customer bigint,
    _limit int) RETURNS SETOF prodpol.customers
    LANGUAGE plpgsql
    PARALLEL SAFE
AS $$
DECLARE
    _p_val text;
BEGIN
    -- Get the value of the ordering column for the previous employee (for keyset pagination)
    IF _ordering_key <> 'customer_id' THEN
        SELECT CASE _ordering_key
                   WHEN 'customer_full_name' THEN normalized_name
                   WHEN 'customer_email' THEN normalized_email
                   WHEN 'customer_phone_number' THEN phone_number
                   END
        INTO _p_val
        FROM prodpol.customers
        WHERE customer_id = _previous_customer;
    END IF;

    RETURN QUERY
        SELECT *
        FROM prodpol.employees
        WHERE
            CASE WHEN _asc THEN
                     CASE _ordering_key
                         WHEN 'customer_id'           THEN employee_id > _previous_customer
                         WHEN 'customer_full_name'    THEN (normalized_name, employee_id) > (_p_val, _previous_customer)
                         WHEN 'customer_email'       THEN (normalized_email, employee_id) > (_p_val, _previous_customer)
                         WHEN 'customer_phone_number' THEN (phone_number, employee_id) > (_p_val, _previous_customer)
                         END
                 ELSE
                     CASE _ordering_key
                         WHEN 'customer_id'           THEN employee_id < _previous_customer
                         WHEN 'customer_full_name'    THEN (normalized_name, employee_id) < (_p_val, _previous_customer)
                         WHEN 'customer_email'       THEN (normalized_email, employee_id) < (_p_val, _previous_customer)
                         WHEN 'customer_phone_number' THEN (phone_number, employee_id) < (_p_val, _previous_customer)
                         END
                END
        ORDER BY
            CASE WHEN _asc THEN
                     CASE _ordering_key
                         WHEN 'customer_full_name' THEN normalized_name
                         WHEN 'customer_email' THEN normalized_email
                         WHEN 'customer_phone_number' THEN phone_number
                         END
                END ASC,
            CASE WHEN NOT _asc THEN
                     CASE _ordering_key
                         WHEN 'customer_full_name' THEN normalized_name
                         WHEN 'customer_email' THEN normalized_email
                         WHEN 'customer_phone_number' THEN phone_number
                         END
                END DESC,
            CASE WHEN _asc THEN employee_id END ASC,
            CASE WHEN NOT _asc THEN employee_id END DESC
        LIMIT _limit;
END;
$$;