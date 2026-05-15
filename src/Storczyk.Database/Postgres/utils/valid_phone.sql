create or replace function prodpol.valid_phone_number(arg_phone text)
    returns boolean language sql as $$
select arg_phone ~ '^\+?[1-9][0-9]{7,14}$';
$$