create index if not exists "idx_order_products"
    on "prodpol"."order_products" using brin ("order_id", "product_id", "order_product_Id");