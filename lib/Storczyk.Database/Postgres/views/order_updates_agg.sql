CREATE OR REPLACE VIEW prodpol.order_updates_agg
AS
SELECT ou.order_id, jsonb_agg(
        (json_build_object('update', ou, 'state', os, 'employee', e))) as updates
FROM prodpol.order_updates ou
         LEFT JOIN prodpol.order_states os on ou.state = os.order_state_id
         LEFT JOIN prodpol.employees e on ou.employee_id = e.employee_id
group by ou.order_id;