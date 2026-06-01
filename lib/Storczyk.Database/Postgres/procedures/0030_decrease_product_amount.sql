CREATE OR REPLACE PROCEDURE prodpol.decrease_product_amount(
    IN _product_id bigint,
    IN amount integer
)
LANGUAGE SQL
AS $$
    WITH updated_amount AS (
        UPDATE prodpol.products
            SET available_amount = available_amount - amount
            WHERE products.product_id = _product_id
            RETURNING *)
    INSERT INTO prodpol.product_amount_updates(product_id, amount_current, amount_delta, created_by)
    SELECT product_id, available_amount, $2, null
    FROM updated_amount;
$$;