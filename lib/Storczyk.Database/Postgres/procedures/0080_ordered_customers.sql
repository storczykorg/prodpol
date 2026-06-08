create or replace function prodpol.ordered_customers(
    _ordering_key prodpol.customer_ordering_keys,
    _asc bool) returns SETOF prodpol.customers_with_roles
    language plpgsql
    parallel safe as
$$
begin
    return query select *
                 from prodpol.customers
                 order by case when _asc then case _ordering_key when 'customer_full_name' then normalized_name
                                                                 when 'customer_email' then normalized_email
                                                                 when 'customer_phone_number' then phone_number end end
                     asc,
                          case when not _asc then case _ordering_key when 'customer_full_name' then normalized_name
                                                                     when 'customer_email' then normalized_email
                                                                     when 'customer_phone_number'
                                                                         then phone_number end end desc,
                          case when _asc then customer_id end asc, case when not _asc then customer_id end desc;
end;
$$;

CREATE OR REPLACE FUNCTION prodpol.ordered_customers(
    _ordering_key prodpol.customer_ordering_keys default 'customer_id',
    _asc bool default false,
    _cursor bigint default NULL,
    _limit int default 10,
    _search_term text default null,
    _email_search text default null,
    _phone_search text default null,
    _name_search text default null,
    _company_search text default null,
    _role_ids integer array default '{}',
    _roles text array default '{}'
) RETURNS SETOF prodpol.customers_with_roles
    LANGUAGE plpgsql
    PARALLEL SAFE
AS $$
DECLARE
    _p_val text;
BEGIN
    -- Only fetch previous value if we have a cursor AND we aren't sorting strictly by ID
    IF _cursor IS NOT NULL AND _ordering_key <> 'customer_id' THEN
        SELECT CASE _ordering_key
                   WHEN 'customer_full_name' THEN normalized_name
                   WHEN 'customer_email' THEN normalized_email
                   WHEN 'customer_phone_number' THEN phone_number
                   END
        INTO _p_val
        FROM prodpol.customers_with_roles
        WHERE customer_id = _cursor;
    END IF;

    RETURN QUERY
        SELECT *
        FROM prodpol.customers_with_roles
        WHERE
          -- Keyset Pagination Logic: Ignore entirely if cursor is NULL (First Page)
            (
                _cursor IS NULL OR
                CASE WHEN _asc THEN
                         CASE _ordering_key
                             WHEN 'customer_id'           THEN customer_id > _cursor
                             WHEN 'customer_full_name'    THEN (normalized_name, customer_id) > (_p_val, _cursor)
                             WHEN 'customer_email'       THEN (normalized_email, customer_id) > (_p_val, _cursor)
                             WHEN 'customer_phone_number' THEN (phone_number, customer_id) > (_p_val, _cursor)
                             END
                     ELSE
                         CASE _ordering_key
                             WHEN 'customer_id'           THEN customer_id < _cursor
                             WHEN 'customer_full_name'    THEN (normalized_name, customer_id) < (_p_val, _cursor)
                             WHEN 'customer_email'       THEN (normalized_email, customer_id) < (_p_val, _cursor)
                             WHEN 'customer_phone_number' THEN (phone_number, customer_id) < (_p_val, _cursor)
                             END
                    END
                )

          -- Role Array Filtering Logic
          AND (_role_ids = '{}' OR role = ANY(_role_ids))
          AND (_roles = '{}' OR role_name = ANY(_roles))

          -- Global Filtering Logic
          AND (
            _search_term IS NULL
                OR normalized_name LIKE '%' || lower(_search_term) || '%'
                OR normalized_email LIKE '%' || lower(_search_term) || '%'
                OR phone_number ILIKE '%' || _search_term || '%'
            )

          -- Individual Column Filtering Logic
          AND (_email_search IS NULL OR normalized_email LIKE '%' || lower(_email_search) || '%')
          AND (_name_search IS NULL OR normalized_name LIKE '%' || lower(_name_search) || '%')
          AND (_phone_search IS NULL OR phone_number ILIKE '%' || _phone_search || '%')
          AND (_company_search IS NULL OR company_name ILIKE '%' || _company_search || '%')

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
            -- Tie-breaker
            CASE WHEN _asc THEN customer_id END ASC,
            CASE WHEN NOT _asc THEN customer_id END DESC
        LIMIT _limit;
END;
$$;