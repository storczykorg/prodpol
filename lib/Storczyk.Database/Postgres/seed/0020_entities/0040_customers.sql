INSERT INTO prodpol.customers
(customer_id, email, name_first, phone_number, name_last)
VALUES
    (10, 'afufijnp11@example.com', 'Adrianna', '+48671839973', 'Sobczak'),
    (11, 'ssdkvivs75@example.com', 'Magdalena', '+48783200228', 'Górska'),
    (12, 'iqusdqig87@example.com', 'Tymoteusz', '+48799865096', 'Malinowska'),
    (13, 'hfaqmmes7@example.com', 'Oliwier', '+48883736250', 'Czarnecki'),
    (14, 'qcmdqoru56@example.com', 'Florian', '+48669036269', 'Konieczny'),
    (15, 'khrpaxvh11@example.com', 'Emiliia', '+48887875624', 'Krupa'),
    (16, 'gxfklsgo83@example.com', 'Konstanty', '+48537433267', 'Majewska'),
    (17, 'tsdxlpaa82@example.com', 'Kamil', '+48881236269', 'Duda'),
    (18, 'mziaqqkn11@example.com', 'Borys', '+48676793813', 'Sikorski'),
    (19, 'weijirzv78@example.com', 'Edward', '+48694838343', 'Nowakowski')
ON CONFLICT DO NOTHING;