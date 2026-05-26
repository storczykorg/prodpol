/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

create or replace function prodpol.valid_postal_code(arg_postal text)
    returns boolean
    language sql
BEGIN ATOMIC
select arg_postal ~ '^\d{2}-\d{3}$';
END;