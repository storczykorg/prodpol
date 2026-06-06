CREATE OR REPLACE VIEW prodpol.order_products_joined
AS
    SELECT "order_product_Id",
           order_id,
           total_cost,
           amount,
           cost,
           customer_notes,
           product_name,
           price,
           unit_type,
           available_amount,
           unit_base
    FROM prodpol.order_products
    LEFT JOIN prodpol.products p on p.product_id = order_products.product_id;