/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CALL add_constraint('prodpol.product_categories',
                    'FK_product_categories_itself',
                    'FOREIGN KEY (parent_id) ' ||
                    'REFERENCES prodpol.product_categories (category_id)', TRUE);