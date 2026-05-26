-- Root categories
INSERT INTO prodpol.product_categories
    (category_id, display_name, category_name)
VALUES (1, 'Materiały', 'material'),
       (2, 'Dodatki', 'addons'),
       (3, 'Pakiety', 'bundles'),
       (4, 'Produkty', 'products'),
       (5, 'Test', 'test')
ON CONFLICT DO NOTHING;

-- Material categories
INSERT INTO prodpol.product_categories
    (parent_id, category_id, display_name, category_name)
VALUES (1, 10, 'Stal', 'steel'),
       (1, 11, 'Aluminium', 'aluminium'),
       (1, 12, 'Miedź', 'copper'),
       (1, 13, 'Inne Materiały', 'materials_other')
ON CONFLICT DO NOTHING;

-- Addon categories
INSERT INTO prodpol.product_categories
    (parent_id, category_id, display_name, category_name)
VALUES (2, 20, 'Inne Dodatki', 'addons_other'),
       (2, 21, 'Opakowania', 'packaging'),
       (2, 22, 'Grawerunki', 'engravings'),
       (2, 23, 'Obróbka chemiczna', 'chemical_processing')
ON CONFLICT DO NOTHING;

-- Product categories
INSERT INTO prodpol.product_categories
    (parent_id, category_id, display_name, category_name)
VALUES (4, 30, 'Komponenty', 'components'),
       (4, 31, 'Inne Produkty', 'products_other'),
       (4, 32, 'Śruby', 'Screws')
ON CONFLICT DO NOTHING;