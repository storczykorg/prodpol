CREATE OR REPLACE VIEW prodpol.customer_spending_report AS
WITH order_totals AS (
    SELECT 
        o.customer_id,
        o.order_id,
        (COALESCE(SUM(op.total_cost), 0::money) + COALESCE(dm.base_cost, 0::money)) as total_order_value
    FROM prodpol.orders o
    LEFT JOIN prodpol.order_products op ON o.order_id = op.order_id
    LEFT JOIN prodpol.delivery_methods dm ON o.delivery_method = dm.delivery_id
    GROUP BY o.customer_id, o.order_id, dm.base_cost
)
SELECT 
    c.customer_id,
    c.full_name,
    c.email,
    COUNT(ot.order_id) as total_orders,
    SUM(ot.total_order_value) as total_spent,
    AVG(ot.total_order_value) as average_order_value,
    MAX(ot.total_order_value) as most_expensive_order,
    (SELECT MAX(created_at) FROM prodpol.orders WHERE customer_id = c.customer_id) as last_order_date
FROM prodpol.customers c
JOIN order_totals ot ON c.customer_id = ot.customer_id
GROUP BY c.customer_id, c.full_name, c.email;
