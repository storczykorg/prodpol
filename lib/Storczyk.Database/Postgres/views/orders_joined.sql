create or replace view prodpol.order_joined
as
select orders.order_id,
       customer_id,
       created_at,
       employee_id,
       delivery_method,
       total,
       current_state,
       display_name,
       state_name
from prodpol.orders
         left join prodpol.order_states os on orders.current_state = os.order_state_id;