/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

create or replace function prodpol.valid_email(arg_email text)
    returns boolean
    language sql
BEGIN ATOMIC
select arg_email ~ '^(([a-zA-Z0-9\-_.+/]+)|("([+.a-zA-Z0-9_\-]+)"))+@([a-zA-Z]+)(\.([a-zA-Z])+)*$';
END;