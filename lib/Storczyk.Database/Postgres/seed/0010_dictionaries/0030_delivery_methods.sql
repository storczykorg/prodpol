INSERT INTO prodpol.delivery_methods
    (delivery_id, base_cost, delivery_name)
VALUES (1, 100, 'Paczopol'),
       (2, 50, 'Kurier'),
       (3, 0, 'Odbiór w sklepie'),
       (4, 67, 'Sraczkomat')
ON CONFLICT DO NOTHING;