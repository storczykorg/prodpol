create index if not exists idx_customer_customer_roles
on prodpol.customers using btree (role, customer_id)