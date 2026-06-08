create index if not exists idx_order_updates_orders
on prodpol.order_updates using btree (order_id, update_id);
create index if not exists idx_order_updates_employees
    on prodpol.order_updates using btree (employee_id, order_id, update_id);