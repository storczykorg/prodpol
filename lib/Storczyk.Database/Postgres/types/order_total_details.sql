DO $$ BEGIN
    CREATE TYPE prodpol.order_total_details AS
    (
        orders_total money,
        delivery_fee money,
        items_total money
    );
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;