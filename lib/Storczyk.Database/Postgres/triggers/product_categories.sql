create or replace function prodpol.update_product_category_ancestry() returns trigger
    language plpgsql
as
$$
    -- Przelicz `category_path` dla rekordu `NEW` oraz wszystkich jego potomków
DECLARE
    rec RECORD;
    cur_id INTEGER;
    cur_parent INTEGER;
    cur_display TEXT;
    parts TEXT[];
BEGIN
    -- Przelicz `ancestry` dla rekordu `NEW` oraz wszystkich jego potomków
    FOR rec IN (
        WITH RECURSIVE descendants AS (
            SELECT category_id, parent_id, display_name FROM prodpol.product_categories WHERE category_id = NEW.category_id
            UNION ALL
            SELECT p.category_id, p.parent_id, p.display_name
            FROM prodpol.product_categories p
                     JOIN descendants d ON p.parent_id = d.category_id
        )
        SELECT category_id FROM descendants
    ) LOOP
            -- zbuduj ścieżkę od tego węzła w górę do korzenia
            parts := ARRAY[]::varchar[];
            cur_id := rec.category_id;
            LOOP
                SELECT category_name, parent_id INTO cur_display, cur_parent
                FROM prodpol.product_categories WHERE category_id = cur_id;
                IF cur_display IS NULL THEN
                    EXIT;
                END IF;
                parts := array_prepend(cur_display, parts);
                IF cur_parent IS NULL THEN
                    EXIT;
                END IF;
                cur_id := cur_parent;
            END LOOP;
            UPDATE prodpol.product_categories
            SET category_path = parts
            WHERE category_id = rec.category_id;
        END LOOP;

    RETURN NULL; -- trigger typu AFTER
END;
$$;

drop trigger IF EXISTS trg_product_categories_ancestry on prodpol.product_categories;
create trigger trg_product_categories_ancestry
    after insert or update
        of parent_id, display_name
    on prodpol.product_categories
    for each row
execute procedure prodpol.update_product_category_ancestry();