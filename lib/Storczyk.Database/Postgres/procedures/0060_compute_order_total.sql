CREATE OR REPLACE FUNCTION prodpol.compute_order_total(_order_id bigint) RETURNS prodpol.order_total_details
LANGUAGE SQL IMMUTABLE PARALLEL SAFE
BEGIN ATOMIC
    WITH totals AS (
        SELECT
            COALESCE((SELECT SUM(total_cost) FROM prodpol.order_products WHERE order_id = $1), 0::money) AS items_total,
            COALESCE((SELECT base_cost FROM prodpol.delivery_methods WHERE delivery_id = (SELECT delivery_method FROM prodpol.orders WHERE order_id = $1)), 0::money) AS delivery_fee
    )
    SELECT (items_total + delivery_fee), delivery_fee, items_total
    FROM totals;
END;

