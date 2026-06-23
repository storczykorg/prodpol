/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

create or replace function prodpol.reset_email_confirmed() returns trigger
    language plpgsql
as
$$
BEGIN
    NEW.email_confirmed := false;
    RETURN NEW;
END;
$$;

drop trigger IF EXISTS trg_employees_email_confirmed on prodpol.employees;
create trigger trg_employees_email_confirmed
    before update of email
    on prodpol.employees
    for each row
    when (OLD.email IS DISTINCT FROM NEW.email)
execute function prodpol.reset_email_confirmed();
