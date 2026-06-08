create or replace function prodpol.search_orders(
    _employee_ids bigint array default '{}',
    _customer_ids bigint array default '{}',
    _created_from timestamp default null,
    _created_to timestamp default null,
    _states_ids integer[] default '{}',
    _states text[] default '{}',
    _asc boolean default false,
    _cursor bigint default -1,
    _order_by prodpol.order_ordering_keys default 'order_id',
    _limit integer default 10
) returns setof prodpol.order_joined
    language plpgsql as
$$
begin
    return query select order_id,
                        customer_id,
                        created_at,
                        employee_id,
                        delivery_method,
                        total,
                        current_state,
                        display_name,
                        state_name
                 from prodpol.order_joined
                 where (_employee_ids = '{}' or employee_id = any (_employee_ids))
                   and (_customer_ids = '{}' or customer_id = any (_customer_ids))
                   and (_states = '{}' or state_name = any (_states))
                   and (_states_ids = '{}' or current_state = any (_states_ids))

                   and (_created_from is null or created_at >= _created_from)
                   and (_created_to is null or created_at <= _created_to)

                   and (_cursor = -1 or (_asc and order_id > _cursor) or (not _asc and order_id < _cursor))

                 order by case when _asc and _order_by = 'order_id' then order_id end asc,
                          case when _asc and _order_by = 'customer_id' then customer_id end asc,
                          case when _asc and _order_by = 'employee_id' then employee_id end asc,
                          case when _asc and _order_by = 'order_state' then current_state end asc,
                          case when _asc and _order_by = 'order_total' then total end asc,
                          case when not _asc and _order_by = 'order_id' then order_id end desc,
                          case when not _asc and _order_by = 'customer_id' then customer_id end desc,
                          case when not _asc and _order_by = 'employee_id' then employee_id end desc,
                          case when not _asc and _order_by = 'order_state' then current_state end desc,
                          case when not _asc and _order_by = 'order_total' then total end desc, order_id asc
                limit _limit;
end;
$$;