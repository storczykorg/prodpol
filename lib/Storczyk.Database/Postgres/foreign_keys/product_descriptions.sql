CALL add_constraint('prodpol.product_descriptions',
                      'FK_product_descriptions_products',
                      'foreign key (product_id) ' ||
                      'references prodpol.products (product_id) ' ||
                      'on delete cascade on update cascade', TRUE);