create or replace function prodpol.filtered_employees(
    _fullname varchar default null,
    _email varchar default null,
    _phone_number varchar default null,
    _role_names varchar array default null
) returns SETOF prodpol.employees_with_roles
    language plpgsql
    parallel safe as
$$
begin

    return query select employee_id,
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
                 from prodpol.employees_with_roles
                 where position(coalesce(_fullname, '') in normalized_name) > 0
                   and position(coalesce(_email, '') in email) > 0
                   and position(coalesce(_phone_number, '') in phone_number) > 0
                   and (coalesce(array_length(_role_names, 1) = 0, true) or
                        exists ( select 1 from unnest(_role_names) as r where position(r in role_name) > 0 ));
end;
$$;

create or replace function prodpol.filtered_employees(
    _cursor_id bigint,
    _asc boolean,
    _fullname varchar default null,
    _email varchar default null,
    _phone_number varchar default null,
    _role_names varchar array default null
) returns SETOF prodpol.employees_with_roles
    language plpgsql
    parallel safe as
$$
begin

    return query select employee_id,
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
                 from prodpol.employees_with_roles
                 where position(coalesce(_fullname, '') in normalized_name) > 0
                   and position(coalesce(_email, '') in email) > 0
                   and position(coalesce(_phone_number, '') in phone_number) > 0
                   and (coalesce(array_length(_role_names, 1) = 0, true) or
                        exists ( select 1 from unnest(_role_names) as r where position(r in role_name) > 0 ) and
                        (_cursor_id is null or
                         (case _asc when true then employee_id > _cursor_id else employee_id < _cursor_id end)));
end;
$$;