INSERT INTO prodpol.customer_roles(role_id, display_name, role_name)
VALUES (1, 'Konta Testowe', 'testing'),
       (2, 'Klienci Hurtowi', 'mass_clients'),
       (3, 'Małe Przedsiębiorstwa', 'small_businesses')
ON CONFLICT DO NOTHING;
