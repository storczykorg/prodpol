create or replace function prodpol.valid_postal_code(arg_postal text)
    returns boolean language sql as $$
select arg_postal ~ '^\d{2}-\d{3}$';
$$