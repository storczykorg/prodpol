/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CALL add_constraint('prodpol.product_amount_updates',
                    'FK_product_amount_updates_products',
                    'FOREIGN KEY (product_id) ' ||
                    'REFERENCES prodpol.products',
                    TRUE);
CALL add_constraint('prodpol.product_amount_updates',
                    'FK_product_amount_updates_employees',
                    'FOREIGN KEY (created_by) ' ||
                    'REFERENCES prodpol.employees (employee_id)',
                    TRUE);