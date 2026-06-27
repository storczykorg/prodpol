namespace Storczyk.Prodpol.Core.Services

open System
open Dapper
open Npgsql
open Storczyk.Async
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Utils

[<RegisterAsTransient(typeof<ICustomerSearchRepository>)>]
type PgCustomerSearchRepository(dataSource: NpgsqlDataSource) =

    interface ICustomerSearchRepository with
        member this.CountSearchAsync(options, token) =
            async {
                use! conn = dataSource.OpenConnectionAsync(token)

                let fullName = options.fullName |> Option.defaultValue ""
                let email = options.email |> Option.defaultValue ""
                let phoneNumber = options.phoneNumber |> Option.defaultValue ""

                let roleNames =
                    options.roleNames |> Option.defaultValue [||] |> Array.map (fun n -> n :> obj)

                let companyName = options.companyName |> Option.defaultValue ""

                let param =
                    {| _search_term =
                        if
                            String.IsNullOrEmpty(fullName)
                            && String.IsNullOrEmpty(email)
                            && String.IsNullOrEmpty(phoneNumber)
                            && String.IsNullOrEmpty(companyName)
                        then
                            null
                        else
                            fullName
                       _email_search = if String.IsNullOrEmpty(email) then null else email
                       _phone_search =
                        if String.IsNullOrEmpty(phoneNumber) then
                            null
                        else
                            phoneNumber
                       _name_search = if String.IsNullOrEmpty(fullName) then null else fullName
                       _company_search =
                        if String.IsNullOrEmpty(companyName) then
                            null
                        else
                            companyName
                       _role_ids = Array.empty<int>
                       _roles = roleNames
                       _cursor = Nullable<int64>()
                       _limit = 0
                       _ordering_key = "customer_id"
                       _asc = false |}

                let! total =
                    wrapTask (
                        conn.QuerySingleAsync<int>(
                            "SELECT COUNT(*) FROM prodpol.ordered_customers(
                             _ordering_key := @_ordering_key::prodpol.customer_ordering_keys,
                             _asc := @_asc,
                             _cursor := @_cursor,
                             _limit := @_limit,
                             _search_term := @_search_term,
                             _email_search := @_email_search,
                             _phone_search := @_phone_search,
                             _name_search := @_name_search,
                             _company_search := @_company_search,
                             _role_ids := @_role_ids,
                             _roles := @_roles);",
                            param
                        )
                    )

                return total
            }

        member this.SearchAsync(options, token) =
            async {
                use! conn = dataSource.OpenConnectionAsync(token)

                let fullName = options.fullName |> Option.defaultValue ""
                let email = options.email |> Option.defaultValue ""
                let phoneNumber = options.phoneNumber |> Option.defaultValue ""
                let companyName = options.companyName |> Option.defaultValue ""

                let roleNames =
                    options.roleNames |> Option.defaultValue [||] |> Array.map (fun n -> n :> obj)

                let orderKey = options.orderBy |> Option.defaultValue CustomerOrderKeys.CustomerId
                let orderKeyStr = CustomerUtilHelpers.GetSqlName orderKey

                let cursor =
                    if options.skip > 0 then
                        Nullable<int64>(int64 options.skip)
                    else
                        Nullable<int64>()

                let limit = options.limit

                let param =
                    {| _search_term =
                        if
                            String.IsNullOrEmpty(fullName)
                            && String.IsNullOrEmpty(email)
                            && String.IsNullOrEmpty(phoneNumber)
                            && String.IsNullOrEmpty(companyName)
                        then
                            null
                        else
                            fullName
                       _email_search = if String.IsNullOrEmpty(email) then null else email
                       _phone_search =
                        if String.IsNullOrEmpty(phoneNumber) then
                            null
                        else
                            phoneNumber
                       _name_search = if String.IsNullOrEmpty(fullName) then null else fullName
                       _company_search =
                        if String.IsNullOrEmpty(companyName) then
                            null
                        else
                            companyName
                       _role_ids = Array.empty<int>
                       _roles = roleNames
                       _cursor = cursor
                       _limit = limit
                       _ordering_key = orderKeyStr
                       _asc = options.asc |}

                let! results =
                    wrapTask (
                        conn.QueryAsync<CustomerRead>(
                            "SELECT * FROM prodpol.ordered_customers(
                             _ordering_key := @_ordering_key::prodpol.customer_ordering_keys,
                             _asc := @_asc,
                             _cursor := @_cursor,
                             _limit := @_limit,
                             _search_term := @_search_term,
                             _email_search := @_email_search,
                             _phone_search := @_phone_search,
                             _name_search := @_name_search,
                             _company_search := @_company_search,
                             _role_ids := @_role_ids,
                             _roles := @_roles);",
                            param
                        )
                    )

                let resultsArr = results |> Seq.toArray

                let! total =
                    wrapTask (
                        conn.QuerySingleAsync<int>(
                            "SELECT COUNT(*) FROM prodpol.ordered_customers(
                             _ordering_key := @_ordering_key::prodpol.customer_ordering_keys,
                             _asc := @_asc,
                             _cursor := @_cursor,
                             _limit := 0,
                             _search_term := @_search_term,
                             _email_search := @_email_search,
                             _phone_search := @_phone_search,
                             _name_search := @_name_search,
                             _company_search := @_company_search,
                             _role_ids := @_role_ids,
                             _roles := @_roles);",
                            param
                        )
                    )

                let nextCursor =
                    if
                        resultsArr.Length > 0
                        && int64 options.skip + int64 resultsArr.Length < int64 total
                    then
                        Some(int64 options.skip + int64 resultsArr.Length)
                    else
                        None

                return
                    CustomerSearchResult(
                        results = (resultsArr :> seq<CustomerRead>),
                        nextCursor = nextCursor,
                        total = int64 total
                    )
            }
