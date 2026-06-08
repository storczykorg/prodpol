--- generate orders

do
$$
    begin
        for i in 0..80
            loop
                insert
                into prodpol.orders (customer_id, employee_id, delivery_method, total, current_state,
                                     created_at)
                select c.customer_id,
                       emp.employee_id, -- Dynamically pulls a random employee
                       delivery.delivery_id,
                       (0, 0, 0)::prodpol.order_total_details,
                       states.order_state_id,
                       now() - (random() * '5 years'::interval)
                from prodpol.customers c
                         cross join lateral (select * from prodpol.order_states order by random() limit 2) states
                         cross join lateral (select * from prodpol.delivery_methods order by random() limit 1) delivery
                         cross join lateral (select employee_id from prodpol.employees order by random() limit 1) emp -- Added random employee selection
                group by (c.customer_id, delivery_id), states.order_state_id, emp.employee_id
                order by random()
                limit 3
                on conflict do nothing;
            end loop;
    end;
$$ language plpgsql;
--- generate order items
do
$$
    begin
        for i in 0..100
            loop

                with items as ( select *, (trunc(random() * 1000))::integer as ammount from prodpol.products )

                insert
                into prodpol.order_products (order_id, total_cost, amount, cost, product_id, customer_notes)
                select o.order_id, items.price * ammount, ammount, items.price, product_id, 'test'
                from items
                         cross join lateral (select * from prodpol.orders o order by random() limit 5) o
                order by random()
                limit 20
                on conflict do nothing;
            end loop;
    end;
$$ language plpgsql;

--- update total prices
update prodpol.orders
set total = prodpol.compute_order_total(order_id);

--- generate order updates
do
$$
    begin
        for i in 0..256
            loop
                with emp as ( select employee_id from prodpol.employees order by random() limit 1 ),
                     o as ( select order_id from prodpol.orders order by random() limit 20 ),
                     state as ( select order_state_id from prodpol.order_states order by random() limit 2 )
                insert
                into prodpol.order_updates(order_id, employee_id, state, created_at)
                select o.order_id,
                       emp.employee_id,
                       state.order_state_id,
                       now() + ((state.order_state_id - 20) * '5 days'::interval)
                from emp,
                     o,
                     state;
            end loop;


        with newest_order as ( select order_id,
                                      state                                                             as newest_state,
                                      row_number() over (partition by order_id order by update_id desc) as rn
                               from prodpol.order_updates )
        update prodpol.orders o
        set current_state = no.newest_state
        from newest_order no
        where o.order_id = no.order_id
          and no.rn = 1;


    end;
$$ language plpgsql;