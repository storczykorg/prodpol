create or replace view prodpol.products_omnibus as
with PriceIntervals as ( select product_id,
                                price,
                                modified_at                                           as valid_from,
                                lead(modified_at, 1, current_timestamp)
                                over ( partition by product_id order by modified_at ) as valid_to
                         from prodpol.product_price_updates ),
     LowestPrices as ( select distinct on (product_id) product_id, min(price) as lowest_price, valid_from, valid_to
                       from PriceIntervals
                       where valid_from <= current_timestamp
                         and valid_to >= current_timestamp - interval '30 days'
                       group by product_id, valid_from, valid_to, price
                       order by product_id, price asc, valid_from desc )
select p.*, lp.valid_from lowest_price_to, lp.valid_to lowest_price_from, lp.lowest_price
from LowestPrices lp
         right join prodpol.products p on lp.product_id = p.product_id;