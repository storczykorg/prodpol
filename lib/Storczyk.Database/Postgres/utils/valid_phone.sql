/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

create or replace function prodpol.valid_phone_number(arg_phone text)
    returns boolean
    language sql
AS $$
select arg_phone ~ '^\+?[1-9][0-9]{7,14}$';
$$;