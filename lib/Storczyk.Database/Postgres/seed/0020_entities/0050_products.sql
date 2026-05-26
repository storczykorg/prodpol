INSERT INTO prodpol.products
(product_id, created_at, created_by, last_modified_by,
 last_modified_at, price, unit_type,
 available_amount, unit_base,
 product_name)
VALUES (2, '2026-05-25 09:11:57.690469', 26, 26,
        '2026-05-25 09:11:57.690469', '$67.00', 1, 10000, 1, 'test.produkt'),
       (8, '2026-05-25 10:00:00.000000', 26, 26,
        '2026-05-25 10:00:00.000000', '$120.00', 1, 500, 1, 'test.part'),
       (16, '2026-05-25 10:05:00.000000', 26, 26,
        '2026-05-25 10:05:00.000000', '$45.50', 2, 200, 1, 'test.material')
ON CONFLICT DO NOTHING;
