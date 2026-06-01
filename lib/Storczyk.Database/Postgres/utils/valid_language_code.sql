/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

create or replace function prodpol.valid_language_code(lang_code text)
    returns boolean
    language sql
AS $$
select lang_code ~ '^[a-z]{2}(-[a-z]{2})?$';
$$;