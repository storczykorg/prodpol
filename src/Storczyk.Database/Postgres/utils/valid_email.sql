create or replace function prodpol.valid_email(arg_email text)
    returns boolean language sql as $$
select arg_email ~ '^(([a-zA-Z\-_.+/]+)|("([+.a-zA-Z_\-]+)"))+@([a-zA-Z]+)(\.([a-zA-Z])+)*$';
$$