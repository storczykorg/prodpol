/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CALL add_constraint(
        'prodpol.products',
        'FK_products_created_by_employees',
        'FOREIGN KEY (created_by) ' ||
        'REFERENCES prodpol.employees(employee_id)' ||
        'ON DELETE SET NULL ON UPDATE CASCADE', TRUE
     );


CALL add_constraint(
        'prodpol.products',
        'FK_products_modified_by_employees',
        'FOREIGN KEY (last_modified_by) ' ||
        'REFERENCES prodpol.employees(employee_id)' ||
        'ON DELETE SET NULL ON UPDATE CASCADE', TRUE
     );

CALL prodpol.add_constraint(
        'prodpol.products',
        'CK_products_price',
        'check ( price >= 0::money )');
CALL prodpol.add_constraint(
        'prodpol.products',
        'CK_products_available_amount',
        'check ( available_amount >= 0 )');
CALL prodpol.add_constraint(
        'prodpol.products',
        'CK_products_last_modified_at',
        'check ( last_modified_at >= created_at )');
