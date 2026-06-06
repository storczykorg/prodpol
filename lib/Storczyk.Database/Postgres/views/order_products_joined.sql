CREATE OR REPLACE VIEW orders_with_states
AS
    SELECT *
    FROM prodpol.orders
    LEFT JOIN prodpol.order_states os on os.order_state_id = orders.current_state;