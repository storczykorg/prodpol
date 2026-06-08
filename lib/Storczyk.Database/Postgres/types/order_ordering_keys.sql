DO $$ BEGIN
    CREATE TYPE prodpol.order_ordering_keys AS enum(
        'order_id',
        'order_state',
        'order_total',
        'customer_id',
        'employee_id'
        );
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;