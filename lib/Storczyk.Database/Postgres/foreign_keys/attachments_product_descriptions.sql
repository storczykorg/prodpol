/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CALL add_constraint('prodpol.attachments_product_descriptions',
                    'FK_attachments_product_descriptions_attachments',
                    'FOREIGN KEY (attachments_attachment_id) ' ||
                    'REFERENCES prodpol.attachments (attachment_id)' ||
                    'ON DELETE CASCADE ' ||
                    'ON UPDATE CASCADE', TRUE);
CALL add_constraint('prodpol.attachments_product_descriptions',
                    'FK_attachments_product_descriptions_product_descriptions',
                    'FOREIGN KEY (product_descriptions_product_id) ' ||
                    'REFERENCES prodpol.product_descriptions (product_id)' ||
                    'ON DELETE CASCADE ' ||
                    'ON UPDATE CASCADE', TRUE);
CALL add_constraint('prodpol.attachments_product_descriptions',
                    'FK_attachments_product_descriptions_product_attachment_types',
                    'FOREIGN KEY (type) ' ||
                    'REFERENCES prodpol.product_attachment_types (type_id)' ||
                    'ON DELETE RESTRICT ' ||
                    'ON UPDATE CASCADE', TRUE);