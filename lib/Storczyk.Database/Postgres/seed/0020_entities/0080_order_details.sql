insert into prodpol.order_details (
    order_id,
    first_name,
    last_name,
    company_name,
    "NIP_code",
    city,
    street,
    zip_code,
    street_no,
    phone_number
)
select
    o.order_id,
    c.name_first,
    c.name_last,

    -- 10% chance to be a company. If true, use customer data or generate a mock company.
    case when rc.is_company
             then coalesce(c.company_name, (array['Pol-Bud Sp. z o.o.', 'Tech-Meb', 'Januszex', 'Trans-Max S.A.', 'Giga-Soft'])[floor(random() * 5) + 1])
         else null
        end,

    -- If it's a company, assign a NIP (fallback to a random 10-digit number)
    case when rc.is_company
             then floor(random() * 9000000000 + 1000000000)::text
         else null
        end,

    -- Assign random address as previously requested
    (array['Warszawa', 'Krakow', 'Wroclaw', 'Poznan', 'Gdansk'])[floor(random() * 5) + 1],
    (array['Dluga', 'Krotka', 'Polna', 'Lesna', 'Sloneczna'])[floor(random() * 5) + 1],

    -- Enforce strict zip code format (XX-XXX)
    lpad(floor(random() * 100)::int::text, 2, '0') || '-' || lpad(floor(random() * 1000)::int::text, 3, '0'),

    -- Enforce strict street number format
    floor(random() * 200 + 1)::text,

    -- Pull phone from customer, fallback to valid random phone if NULL
    coalesce(c.phone_number, '+48' || floor(random() * 900000000 + 100000000)::text)

from prodpol.orders o
         left join prodpol.customers c on o.customer_id = c.customer_id
-- Generate a boolean that is TRUE roughly 10% of the time for each row
         cross join lateral (select (random() < 0.10)::boolean as is_company) rc
on conflict (order_id) do nothing;