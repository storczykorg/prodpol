INSERT INTO prodpol.orders
(order_id, customer_id, employee_id, delivery_method, total, current_state)
VALUES (
        0, 
        10, 
        26,
        1,
        (0,0,0),
        1
       ) ON CONFLICT DO NOTHING;
INSERT INTO prodpol.order_products
(order_id, total_cost, amount, cost, product_id, customer_notes) 
VALUES (0, 23, 10, 5, 2, 'test')
ON CONFLICT DO NOTHING;
UPDATE prodpol.orders
    SET total = prodpol.compute_order_total(order_id)
    WHERE order_id = 0;