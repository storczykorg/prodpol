CREATE OR REPLACE FUNCTION prodpol.most_performing_employees (
    _start_date timestamp DEFAULT NULL,
    _end_date timestamp DEFAULT NULL,
    _limit int DEFAULT 10,
    _asc bool DEFAULT false
) RETURNS TABLE (
                    employee_id bigint,
                    update_count bigint,
                    role_id integer,
                    email varchar(128),
                    normalized_email text,
                    name_first varchar(128),
                    name_last varchar(128),
                    full_name text,
                    normalized_name text,
                    phone_number varchar(128),
                    employee_created_at timestamp,
                    role_name varchar(128),
                    role_display_name varchar(128)
                )
    LANGUAGE plpgsql
    PARALLEL SAFE
AS $$
BEGIN
    RETURN QUERY
        SELECT
            e.employee_id,
            COALESCE(agg.update_count, 0) AS update_count,
            e.role_id,
            e.email,
            e.normalized_email,
            e.name_first,
            e.name_last,
            e.full_name,
            e.normalized_name,
            e.phone_number,
            e.created_at as employee_created_at,
            e.role_name,
            e.role_display_name
        FROM prodpol.employees_with_roles e
                 LEFT JOIN (
            SELECT
                ou.employee_id,
                COUNT(ou.update_id) as update_count
            FROM prodpol.order_updates ou
            WHERE ou.employee_id IS NOT NULL
              AND (_start_date IS NULL OR ou.created_at >= _start_date)
              AND (_end_date IS NULL OR ou.created_at <= _end_date)
            GROUP BY ou.employee_id
        ) agg ON e.employee_id = agg.employee_id
        ORDER BY
            CASE WHEN _asc THEN COALESCE(agg.update_count, 0) END ASC,
            CASE WHEN NOT _asc THEN COALESCE(agg.update_count, 0) END DESC,

            CASE WHEN _asc THEN e.employee_id END ASC,
            CASE WHEN NOT _asc THEN e.employee_id END DESC
        LIMIT _limit;
END;
$$;

comment on function prodpol.most_performing_employees(_start_date timestamp, _end_date timestamp, _limit int, _asc bool)
is 'Pick most/least performing employees using update count as a metric';