CREATE OR REPLACE PROCEDURE prodpol.reserve_ordered_product_amount(
    IN _order_id bigint
) LANGUAGE SQL
BEGIN ATOMIC
    WITH aggregated_orders AS (
        SELECT product_id, SUM(amount) AS total_amount
        FROM prodpol.order_products
        WHERE order_id = _order_id
        GROUP BY product_id
    ),
    updated AS (
        UPDATE prodpol.products p
        SET available_amount = p.available_amount - ao.total_amount
        FROM aggregated_orders ao
        WHERE p.product_id = ao.product_id
        RETURNING p.product_id, p.available_amount, ao.total_amount
    )
    INSERT INTO prodpol.product_amount_updates(product_id, amount_current, amount_delta, created_by)
    SELECT product_id, available_amount, total_amount, null
    FROM updated;
END;
