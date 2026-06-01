CREATE OR REPLACE PROCEDURE prodpol.update_product_price(
    _product_id bigint,
    _new_price money,
    _employee_id bigint
)
LANGUAGE SQL
AS $$
    WITH updated AS (
        UPDATE prodpol.products
        SET price            = _new_price,
            last_modified_by = _employee_id,
            last_modified_at = now()
        WHERE product_id = _product_id
        RETURNING last_modified_at
    )
    INSERT INTO prodpol.product_price_updates
    (product_id, price, modified_by, modified_at)
    SELECT _product_id, _new_price, _employee_id, last_modified_at
    FROM updated
    RETURNING price_update_id;
$$;