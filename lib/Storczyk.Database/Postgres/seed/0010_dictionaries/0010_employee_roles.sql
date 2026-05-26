/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

INSERT INTO prodpol.employee_roles (role_id, display_name, role_name)
VALUES (1, 'System Informatyczny', 'special.system'),
       (2, 'Administracja', 'admin'),
       (3, 'Magazyn', 'warehouse'),
       (4, 'Obsługa Klienta', 'customer_service'),
       (5, 'CNC', 'machinist'),
       (7, 'Sprzątanie', 'janitor') ON CONFLICT DO NOTHING;
