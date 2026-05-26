CREATE OR REPLACE PROCEDURE prodpol.create_products_snapshot(product_id bigint)
LANGUAGE SQL
BEGIN ATOMIC
    INSERT INTO prodpol.product_information_snapshots
    (product_id, created, modified, price, unit_type, available_amount, unit_base, product_name, snapshot_at,
     price_updates, amount_updates, description_updates, descriptions)
    SELECT products.product_id,
           JSONB_BUILD_OBJECT(
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
                            WHERE role_id = created.role_id
                            LIMIT 1)
           )                                                  AS created,
           JSONB_BUILD_OBJECT(
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
                            WHERE role_id = modified.role_id
                            LIMIT 1)
           )                                                  AS modified,
           price,
           unit_type,
           available_amount,
           unit_base,
           product_name,
           NOW()                                              AS snapshot_at,
           (SELECT JSONB_AGG(p_updates)
            FROM prodpol.product_price_updates p_updates
            WHERE p_updates.product_id = products.product_id) AS price_updates,
           (SELECT JSONB_AGG(p_updates)
            FROM prodpol.product_amount_updates p_updates
            WHERE p_updates.product_id = products.product_id) AS amount_updates,
           (SELECT JSONB_AGG(d_updates)
            FROM prodpol.product_description_updates d_updates
            WHERE d_updates.product_id = products.product_id) AS description_updates,
           (SELECT JSONB_AGG(d_current)
            FROM prodpol.product_descriptions d_current
            WHERE d_current.product_id = products.product_id) AS descriptions
    FROM prodpol.products
             LEFT JOIN prodpol.employees created ON products.created_by = created.employee_id
             LEFT JOIN prodpol.employees modified ON products.last_modified_by = modified.employee_id
    WHERE products.product_id = $1;
END;
