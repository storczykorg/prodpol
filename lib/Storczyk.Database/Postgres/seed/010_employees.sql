/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

INSERT INTO prodpol.employees (employee_id, role_id, email, name_first, name_last, phone_number, password_hash,
                               created_at, enabled)
VALUES (26, 1, 'system@storczyk.org', 'System', 'Informatyczny', '+48111111111', null, '2026-05-11 16:28:03.522502',
        true),
       (28, 2, 'szefu@prodpol.pl', 'Szymon', 'Szefowski', '+48697710560', null, '2026-05-11 16:30:27.128477', false),
       (27, 2, 'm.madziowska@prodpol.pl', 'Madzia', 'Madziowska', '+48663689080', null, '2026-05-11 16:30:27.128477',
        false),
       (29, 3, 'filip.g@prodpol.pl', 'Filip', 'Gos', '+48727201441', null, '2026-05-11 16:38:16.706108', false),
       (30, 3, 's.sobotka@gmail.com', 'Sabrina', 'Sobotka', '+48721654916', null, '2026-05-11 16:38:16.706108', false),
       (31, 3, 'kubas@gmail.com', 'Flora', 'Kubas', '+48788290281', null, '2026-05-11 16:38:16.706108', false),
       (32, 4, 'grzmiciok@gmail.com', 'Wisława', 'Jania', '+48699378124', null, '2026-05-11 16:38:16.706108', false),
       (33, 4, 'kradziok@gmail.com', 'Bolesława', 'Sałata', '+48889310983', null, '2026-05-11 16:38:16.706108', false),
       (34, 5, 'kajfiok@proton.me', 'Kajfasz', 'Siwek', '+48786519499', null, '2026-05-11 16:38:16.706108', false),
       (35, 5, 'gggustaw@wp.pl', 'Gustaw', 'Tyburski', '+48534556579', null, '2026-05-11 16:38:16.706108',
        false) ON CONFLICT DO NOTHING;
