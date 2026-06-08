/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

insert into prodpol.employees (employee_id, role_id, email, name_first, name_last, phone_number, password_hash,
                               created_at, enabled)
values (26, 1, 'system@storczyk.org', 'System', 'Informatyczny', '+48111111111', null, '2026-05-11 16:28:03.522502',
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
        false),
       (50, 3, 'jan.kowalski@prodpol.pl', 'Jan', 'Kowalski', '+48500100200', null, '2026-06-08 10:00:00', true),
       (51, 3, 'anna.nowak@prodpol.pl', 'Anna', 'Nowak', '+48500100201', null, '2026-06-08 10:05:00', true),
       (52, 4, 'piotr.wisniewski@prodpol.pl', 'Piotr', 'Wisniewski', '+48500100202', null, '2026-06-08 10:10:00', true),
       (53, 4, 'maria.wojcik@prodpol.pl', 'Maria', 'Wojcik', '+48500100203', null, '2026-06-08 10:15:00', true),
       (54, 5, 'lukasz.kowalczyk@prodpol.pl', 'Lukasz', 'Kowalczyk', '+48500100204', null, '2026-06-08 10:20:00', true),
       (55, 5, 'agnieszka.kaminska@prodpol.pl', 'Agnieszka', 'Kaminska', '+48500100205', null, '2026-06-08 10:25:00',
        true),
       (56, 5, 'tomasz.lewandowski@prodpol.pl', 'Tomasz', 'Lewandowski', '+48500100206', null, '2026-06-08 10:30:00',
        true),
       (57, 5, 'katarzyna.zielinska@prodpol.pl', 'Katarzyna', 'Zielinska', '+48500100207', null, '2026-06-08 10:35:00',
        true),
       (58, 7, 'marek.szymanski@prodpol.pl', 'Marek', 'Szymanski', '+48500100208', null, '2026-06-08 10:40:00', true),
       (59, 7, 'beata.wozniak@prodpol.pl', 'Beata', 'Wozniak', '+48500100209', null, '2026-06-08 10:45:00', true),
       (60, 3, 'adam.koziol@prodpol.pl', 'Adam', 'Koziol', '+48500100210', null, '2026-06-08 10:50:00', true)
on conflict do nothing;
