/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CALL add_constraint('prodpol.order_details', 'FK_order_details_orders',
                    'foreign key (order_id)' ||
                    'references prodpol.orders (order_id)' ||
                    'on delete restrict on update cascade', TRUE)