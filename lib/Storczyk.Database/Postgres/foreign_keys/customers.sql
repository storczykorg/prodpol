/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CALL prodpol.add_constraint('prodpol.customers',
                            'FK_customers_customer_roles',
                            'FOREIGN KEY (role) ' ||
                            'REFERENCES prodpol.customer_roles (role_id) ' ||
                            'ON UPDATE CASCADE', TRUE)