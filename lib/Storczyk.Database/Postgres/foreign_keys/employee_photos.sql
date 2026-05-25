/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CALL prodpol.add_constraint('prodpol.employee_photos',
                    'FK_employee_photos_employees',
                    'FOREIGN KEY (employee_id) ' ||
                    'REFERENCES prodpol.employees', TRUE);