/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CALL add_constraint('prodpol.product_descriptions',
                    'FK_product_descriptions_products',
                    'foreign key (product_id) ' ||
                    'references prodpol.products (product_id) ' ||
                    'on delete cascade on update cascade', TRUE);