CREATE OR REPLACE VIEW prodpol.order_summaries
AS
WITH order_info as (SELECT o.order_id,
                           row_to_json(o.*)  as "order",
                           row_to_json(c.*)  as customer,
                           row_to_json(e.*)  as employee,
                           row_to_json(dm.*) as delivery_method,
                           row_to_json(os.*) as current_state,
                           row_to_json(od.*) as order_details
                    FROM prodpol.orders o
                             LEFT JOIN prodpol.order_states os
                                       on os.order_state_id = o.current_state
                             LEFT JOIN prodpol.customers_with_roles c on o.customer_id = c.customer_id
                             LEFT JOIN prodpol.employees_with_roles e on o.employee_id = e.employee_id
                             LEFT JOIN prodpol.delivery_methods dm on o.delivery_method = dm.delivery_id
                             LEFT JOIN prodpol.order_details od on o.order_id = od.order_id),
     order_items as (SELECT order_id, jsonb_agg(o.*) as items FROM prodpol.order_products_joined o group by o.order_id)
SELECT oo.order_id,
       oo."order",
       oo.customer,
       oo.employee,
       oo.delivery_method,
       oo.current_state,
       oo.order_details,
       oi.items,
       up.updates
FROM order_info oo
         LEFT JOIN order_items oi ON oo.order_id = oi.order_id
         LEFT JOIN prodpol.order_updates_agg up ON up.order_id = oo.order_id
;