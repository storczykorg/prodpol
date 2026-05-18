/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CALL add_constraint('prodpol.order_invoice', 'FK_order_details_customers',
                    'foreign key (customer_id)
      references prodpol.customers (customer_id)
      on DELETE CASCADE ON UPDATE CASCADE');
CALL add_constraint('prodpol.order_invoice', 'FK_order_details_orders',
                    'foreign key (order_id)
      references prodpol.orders
      on DELETE CASCADE ON UPDATE CASCADE', TRUE);