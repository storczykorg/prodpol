INSERT INTO prodpol.order_states
    (order_state_id, display_name, state_name) 
VALUES
    (1, 'Nowe', 'new'),
    (2, 'Potwierdzone', 'confirmed'),
    (3, 'Spakowane', 'packed'),
    (4, 'Wysłane', 'send'),
    (5, 'Odebrane', 'received'),
    (6, 'Anulowane', 'cancelled'),
    (7, 'W trakcie zwrotu', 'returning'),
    (8, 'Zwrócone', 'returned')
ON CONFLICT DO NOTHING;