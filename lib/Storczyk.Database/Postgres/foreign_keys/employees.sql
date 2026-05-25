/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CALL prodpol.add_constraint('prodpol.employees', 'FK_employees_roles',
                    'foreign key (role_id) references prodpol.employee_roles (role_id)
                       on delete set null
                          on update cascade')