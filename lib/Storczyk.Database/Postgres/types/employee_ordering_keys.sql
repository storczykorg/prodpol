DO $$ BEGIN
    CREATE TYPE prodpol.employee_ordering_keys AS enum(
        'employee_id',
        'employee_full_name',
        'employee_email',
        'employee_phone_number'
        );
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;