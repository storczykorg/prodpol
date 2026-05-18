/*
 * Copyright 2026 storczyk.org. All rights reserved.
 * This work is licensed under the terms of the MIT license.  
 * For a copy, see <https://opensource.org/licenses/MIT>.
 */

CALL prodpol.add_constraint('prodpol.customers'::text,
                            'CK_format_customers_email'::text,
                            'CHECK (prodpol.valid_email(normalized_email::text))'::text,
                            TRUE);

CALL prodpol.add_constraint('prodpol.customers'::text,
                            'CK_format_customers_phone_number'::text,
                            'CHECK (prodpol.valid_phone_number(phone_number::text))'::text,
                            TRUE);