CREATE OR REPLACE FUNCTION prodpol.filtered_employees(
    _fullname varchar default null,
    _email varchar default null,
    _phone_number varchar default null,
    _role_names varchar array default null,
    _cursor_id bigint default null
) RETURNS SETOF prodpol.employees_with_roles
    LANGUAGE plpgsql
    PARALLEL SAFE
AS
$$
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
        WHERE position(coalesce(_fullname, '') IN normalized_name) > 0
          AND position(coalesce(_email, '') IN email) > 0
          AND position(coalesce(_phone_number, '') IN phone_number) > 0
          AND (coalesce(array_length(_role_names, 1) = 0, TRUE) OR (role_name = ANY (_role_names)))
          AND coalesce(_cursor_id < employee_id, TRUE);
END;
$$