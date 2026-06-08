CREATE OR REPLACE VIEW prodpol.order_updates_agg AS
WITH recent_updates AS (
    SELECT order_id, update_id, total_updates
    FROM (
             SELECT
                 order_id,
                 update_id,
                 ROW_NUMBER() OVER (PARTITION BY order_id ORDER BY update_id DESC) as rn,
                 COUNT(*) OVER (PARTITION BY order_id) as total_updates -- Counts all rows per order
             FROM prodpol.order_updates
         ) sub
    WHERE rn <= 5
)
SELECT
    ou.order_id,
    MAX(ru.total_updates) as total_updates_count,
    jsonb_agg(
            json_build_object('update', ou, 'state', os, 'employee', e)
            ORDER BY ou.update_id DESC
    ) as updates
FROM recent_updates ru
         JOIN prodpol.order_updates ou ON ru.update_id = ou.update_id
         LEFT JOIN prodpol.order_states os ON ou.state = os.order_state_id
         LEFT JOIN prodpol.employees e ON ou.employee_id = e.employee_id
GROUP BY ou.order_id;

comment on view prodpol.order_updates_agg
is 'Aggregated view on showing order''s last updates and all items';