/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CALL add_constraint('prodpol.product_information_snapshots',
                    'FK_product_information_snapshots_products',
                    'FOREIGN KEY (product_id) ' ||
                    'REFERENCES prodpol.products (product_id) ' ||
                    'ON DELETE CASCADE ' ||
                    'ON UPDATE CASCADE', TRUE);
