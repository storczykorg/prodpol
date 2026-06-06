CREATE OR REPLACE VIEW prodpol.products_omnibus
AS
WITH PriceIntervals AS (
    SELECT
        product_id,
        price,
        modified_at AS valid_from,
        LEAD(modified_at, 1, CURRENT_TIMESTAMP) OVER (
            PARTITION BY product_id
            ORDER BY modified_at
            ) AS valid_to
    FROM
        prodpol.product_price_updates
),
     LowestPrices AS (
         SELECT DISTINCT ON (product_id)
             product_id,
             MIN(price) AS lowest_price,
             valid_from,
             valid_to
         FROM
             PriceIntervals
         WHERE
             valid_from <= CURRENT_TIMESTAMP
           AND valid_to >= CURRENT_TIMESTAMP - INTERVAL '30 days'
         group by product_id, valid_from, valid_to, price
         ORDER BY
             product_id,
             price ASC,
             valid_from DESC
     )
SELECT
    p.*,
    lp.valid_from lowest_price_to,
    lp.valid_to lowest_price_from,
    lp.lowest_price
FROM
    LowestPrices lp
        RIGHT JOIN
    prodpol.products p ON lp.product_id = p.product_id;