CREATE OR REPLACE PROCEDURE prodpol.create_all_products_snapshots()
AS
$$
BEGIN
    INSERT INTO prodpol.product_information_snapshots
    (
        product_id,
        created,
        modified,
        price,
        unit_type,
        available_amount,
        unit_base,
        product_name,
        snapshot_at,
        price_updates,
        amount_updates,
        description_updates,
        descriptions
    )
    SELECT products.product_id,
           jsonb_build_object(
                   'at', products.created_at,
                   'by', products.created_by,
                   'employee_id', created.employee_id,
                   'role_id', created.role_id,
                   'email', created.email,
                   'full_name', created.full_name,
                   'phone_number', created.phone_number,
                   'created_at', created.created_at,
                   'enabled', created.enabled,
                   'role', (SELECT role_name
                            FROM prodpol.employee_roles
                            where role_id = created.role_id
                            limit 1)
           )                                                  as created,
           jsonb_build_object(
                   'at', products.last_modified_at,
                   'by', products.last_modified_by,
                   'employee_id', modified.employee_id,
                   'role_id', modified.role_id,
                   'email', modified.email,
                   'full_name', modified.full_name,
                   'phone_number', modified.phone_number,
                   'created_at', modified.created_at,
                   'enabled', modified.enabled,
                   'role', (SELECT role_name
                            FROM prodpol.employee_roles
                            where role_id = modified.role_id
                            limit 1)
           )                                                  as modified,
           price,
           unit_type,
           available_amount,
           unit_base,
           product_name,
           now()                                              as snapshot_at,
           (SELECT jsonb_agg(p_updates)
            FROM prodpol.product_price_updates p_updates
            where p_updates.product_id = products.product_id) as price_updates,
           (SELECT jsonb_agg(p_updates)
            FROM prodpol.product_amount_updates p_updates
            where p_updates.product_id = products.product_id) as amount_updates,
           (SELECT jsonb_agg(d_updates)
            FROM prodpol.product_description_updates d_updates
            where d_updates.product_id = products.product_id) as description_updates,
           (SELECT jsonb_agg(d_current)
            FROM prodpol.product_descriptions d_current
            where d_current.product_id = products.product_id) as descriptions
    FROM prodpol.products
             LEFT JOIN prodpol.employees created on products.created_by = created.employee_id
             LEFT JOIN prodpol.employees modified on products.last_modified_by = modified.employee_id;
END;
$$
    LANGUAGE plpgsql;
