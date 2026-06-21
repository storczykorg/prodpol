create or replace function prodpol.most_performing_employees(
    _start_date timestamp default null,
    _end_date timestamp default null,
    _limit int default 10,
    _asc bool default false
)
    returns TABLE
            (
                employee_id         bigint,
                update_count        bigint,
                role_id             integer,
                email               varchar(128),
                normalized_email    text,
                name_first          varchar(128),
                name_last           varchar(128),
                full_name           text,
                normalized_name     text,
                phone_number        varchar(128),
                employee_created_at timestamp,
                role_name           varchar(128),
                role_display_name   varchar(128)
            )
    language plpgsql
    parallel safe
as
$$
begin
    return query select e.employee_id,
                        coalesce(agg.update_count, 0) as update_count,
                        e.role_id,
                        e.email,
                        e.normalized_email,
                        e.name_first,
                        e.name_last,
                        e.full_name,
                        e.normalized_name,
                        e.phone_number,
                        e.created_at                  as employee_created_at,
                        e.role_name,
                        e.role_display_name
                 from prodpol.employees_with_roles e
                          left join ( select ou.employee_id, count(ou.update_id) as update_count
                                      from prodpol.order_updates ou
                                      where ou.employee_id is not null
                                        and (_start_date is null or ou.created_at >= _start_date)
                                        and (_end_date is null or ou.created_at <= _end_date)
                                      group by ou.employee_id ) agg on e.employee_id = agg.employee_id
                 order by (case when _asc then coalesce(agg.update_count, 0)
                                when not _asc then coalesce(-agg.update_count, 0) end) asc
                 limit _limit;
end;
$$;

comment on function prodpol.most_performing_employees(_start_date timestamp, _end_date timestamp, _limit int, _asc bool) is 'Pick most/least performing employees using update count as a metric';