create or replace function prodpol.get_products_profit_summary(
    _time_from timestamp default null,
    _time_to timestamp default null
)
returns table ( product_id bigint, total_profit float, total_amount float, income_per_item float )
    language plpgsql
    as
$$
begin
    return query select op.product_id,
                        sum(op.total_cost::numeric::float)::float                  "total_profit",
                        sum(op.amount::float)::float                      "total_amount",
                        (sum(op.total_cost::numeric::float) / sum(op.amount::float)) "income_per_item"
                 from prodpol.orders o
                          left join prodpol.order_products op on o.order_id = op.order_id
                 where (_time_from is null or _time_from <= o.created_at)
                   and (_time_to is null or _time_to >= o.created_at)
                 group by op.product_id;
end;

$$;