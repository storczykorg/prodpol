DO $$ BEGIN
    CREATE TYPE prodpol.customer_ordering_keys AS enum(
        'customer_id',
        'customer_full_name',
        'customer_email',
        'customer_phone_number'
        );
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;