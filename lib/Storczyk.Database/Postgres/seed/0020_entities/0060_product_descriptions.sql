INSERT INTO prodpol.product_descriptions (product_id, language_code, title, body, is_public)
VALUES 
    (2, 'en', 'Test Product', 'This is a description for the test product.', true),
    (2, 'pl', 'Produkt Testowy', 'To jest opis produktu testowego.', true),
    (3, 'en', 'Super Gadget', 'A super gadget that solves all your problems.', true),
    (3, 'pl', 'Super Gadżet', 'Super gadżet, który rozwiąże wszystkie Twoje problemy.', true),
    (4, 'en', 'Mega Widget', 'The most powerful widget on the market.', true),
    (4, 'pl', 'Mega Widżet', 'Najpotężniejszy widżet na rynku.', true)
ON CONFLICT DO NOTHING;
