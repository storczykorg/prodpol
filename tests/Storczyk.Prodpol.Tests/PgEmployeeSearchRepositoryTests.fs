module Storczyk.Prodpol.Tests.PgEmployeeSearchRepositoryTests

open System
open System.Linq.Expressions
open System.Reflection
open System.Threading
open System.Threading.Tasks
open NUnit.Framework
open LinqToDB
open LinqToDB.Mapping
open LinqToDB.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Services
open Storczyk.Prodpol.Data.Services.Identity

type SqliteIdentityDatabase(options: DataOptions) =
    inherit DataConnection(options)

    interface IIdentityDatabase with
        member this.EmployeeRead = this.GetTable<EmployeeRead>()

module TestDb =
    let create () : IIdentityDatabase =
        let ms = MappingSchema()

        // SQLite returns Int64 for INTEGER columns, but the model uses int/int option
        let longToIntParam = Expression.Parameter(typeof<int64>, "v")
        let longToIntBody = Expression.Convert(longToIntParam, typeof<int>) :> Expression
        let longToIntLambda = Expression.Lambda(longToIntBody, longToIntParam)
        ms.SetConvertExpression(typeof<int64>, typeof<int>, longToIntLambda, false, ConversionType.FromDatabase)

        let someMethod =
            typeof<int option>
                .GetMethod("Some", BindingFlags.Public ||| BindingFlags.Static, null, [| typeof<int> |], null)

        let longToOptParam = Expression.Parameter(typeof<int64>, "v")

        let longToOptBody =
            Expression.Call(someMethod, Expression.Convert(longToOptParam, typeof<int>)) :> Expression

        let longToOptLambda = Expression.Lambda(longToOptBody, longToOptParam)
        ms.SetConvertExpression(typeof<int64>, typeof<int option>, longToOptLambda, false, ConversionType.FromDatabase)

        FluentMappingBuilder(ms).Entity<EmployeeRead>().HasTableName("employees_with_roles").Build()
        |> ignore

        let options =
            DataOptions().UseSQLite("Data Source=:memory:;Pooling=False").UseMappingSchema(ms)

        let db = new SqliteIdentityDatabase(options)

        db.Execute(
            "
            CREATE TABLE employees_with_roles (
                employee_id INTEGER PRIMARY KEY,
                role_id INTEGER,
                email TEXT NOT NULL,
                email_confirmed INTEGER NOT NULL DEFAULT 0,
                normalized_email TEXT NOT NULL DEFAULT '',
                name_first TEXT NOT NULL,
                name_last TEXT NOT NULL,
                phone_number TEXT NOT NULL,
                password_hash TEXT,
                security_stamp TEXT,
                created_at TEXT NOT NULL,
                enabled INTEGER NOT NULL DEFAULT 0,
                full_name TEXT NOT NULL DEFAULT '',
                normalized_name TEXT NOT NULL DEFAULT '',
                role_name TEXT,
                role_display_name TEXT
            )
        "
        )
        |> ignore

        db.Execute(
            "
            INSERT INTO employees_with_roles VALUES
            (1, 1, 'alice@test.com', 1, 'ALICE@TEST.COM', 'Alice', 'Anderson',
             '+1111111111', NULL, NULL, '2024-01-01T00:00:00Z', 1,
             'Alice Anderson', 'ALICE ANDERSON', 'admin', 'Administrator'),
            (2, 2, 'bob@test.com', 1, 'BOB@TEST.COM', 'Bob', 'Anderson',
             '+1222222222', NULL, NULL, '2024-01-01T00:00:00Z', 1,
             'Bob Anderson', 'BOB ANDERSON', 'user', 'User'),
            (3, 1, 'charlie@test.com', 1, 'CHARLIE@TEST.COM', 'Charlie', 'Brown',
             '+1333333333', NULL, NULL, '2024-01-01T00:00:00Z', 1,
             'Charlie Brown', 'CHARLIE BROWN', 'admin', 'Administrator'),
            (4, 2, 'diana@test.com', 1, 'DIANA@TEST.COM', 'Diana', 'Brown',
             '+1444444444', NULL, NULL, '2024-01-01T00:00:00Z', 1,
             'Diana Brown', 'DIANA BROWN', 'user', 'User'),
            (5, 3, 'eve@test.com', 1, 'EVE@TEST.COM', 'Eve', 'Davis',
             '+1555555555', NULL, NULL, '2024-01-01T00:00:00Z', 1,
             'Eve Davis', 'EVE DAVIS', 'manager', 'Manager'),
            (6, NULL, 'frank@test.com', 1, 'FRANK@TEST.COM', 'Frank', 'Evans',
             '+1666666666', NULL, NULL, '2024-01-01T00:00:00Z', 1,
             'Frank Evans', 'FRANK EVANS', NULL, NULL),
            (7, 1, 'grace@test.com', 1, 'GRACE@TEST.COM', 'Grace', 'Smith',
             '+1777777777', NULL, NULL, '2024-01-01T00:00:00Z', 1,
             'Grace Smith', 'GRACE SMITH', 'admin', 'Administrator'),
            (8, 2, 'grace.smith@test.com', 1, 'GRACE.SMITH@TEST.COM', 'Grace', 'Smith',
             '+1888888888', NULL, NULL, '2024-01-01T00:00:00Z', 1,
             'Grace Smith', 'GRACE SMITH', 'user', 'User')
        "
        )
        |> ignore

        db :> IIdentityDatabase

    let createRepository () : IEmployeeSearchRepository =
        PgEmployeeSearchRepository(create ()) :> IEmployeeSearchRepository

[<Test>]
let ``All filters empty returns all employees`` () =
    let repo = TestDb.createRepository ()
    let options = EmployeeSearchOption(limit = 50, skip = 0, asc = true)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    Assert.That(result.total, Is.EqualTo(8L))
    Assert.That(result.results |> Seq.length, Is.EqualTo(8))

[<Test>]
let ``Filter by fullName substring`` () =
    let repo = TestDb.createRepository ()

    let options =
        EmployeeSearchOption(fullName = Some "ANDERSON", limit = 50, asc = true)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    Assert.That(result.total, Is.EqualTo(2L))
    let ids = result.results |> Seq.map (fun e -> e.Id) |> set
    Assert.That(ids, Is.EquivalentTo([ 1L; 2L ]))

[<Test>]
let ``Filter by email substring`` () =
    let repo = TestDb.createRepository ()
    let options = EmployeeSearchOption(email = Some "alice", limit = 50)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    Assert.That(result.total, Is.EqualTo(1L))
    let emp = result.results |> Seq.head
    Assert.That(emp.Id, Is.EqualTo(1L))

[<Test>]
let ``Filter by phoneNumber substring`` () =
    let repo = TestDb.createRepository ()
    let options = EmployeeSearchOption(phoneNumber = Some "555", limit = 50)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    Assert.That(result.total, Is.EqualTo(1L))
    let emp = result.results |> Seq.head
    Assert.That(emp.Id, Is.EqualTo(5L))

[<Test>]
let ``Filter by role name`` () =
    let repo = TestDb.createRepository ()
    let options = EmployeeSearchOption(roleNames = Some [| "admin" |], limit = 50)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    Assert.That(result.total, Is.EqualTo(3L))
    let ids = result.results |> Seq.map (fun e -> e.Id) |> set
    Assert.That(ids, Is.EquivalentTo([ 1L; 3L; 7L ]))

[<Test>]
let ``Filter combination narrows results`` () =
    let repo = TestDb.createRepository ()

    let options =
        EmployeeSearchOption(fullName = Some "BROWN", roleNames = Some [| "user" |], limit = 50)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    Assert.That(result.total, Is.EqualTo(1L))
    let emp = result.results |> Seq.head
    Assert.That(emp.Id, Is.EqualTo(4L))

[<Test>]
let ``Filter no match returns empty`` () =
    let repo = TestDb.createRepository ()
    let options = EmployeeSearchOption(fullName = Some "NONEXISTENT", limit = 50)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    Assert.That(result.total, Is.EqualTo(0L))
    Assert.That(result.results |> Seq.length, Is.EqualTo(0))

[<Test>]
let ``Order by FullName ascending`` () =
    let repo = TestDb.createRepository ()

    let options =
        EmployeeSearchOption(orderBy = Some EmployeeOrderKeys.FullName, asc = true, limit = 50)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    let ids = result.results |> Seq.map (fun e -> e.Id) |> Seq.toArray
    Assert.That(ids[0], Is.EqualTo(1L))

[<Test>]
let ``Order by FullName descending`` () =
    let repo = TestDb.createRepository ()

    let options =
        EmployeeSearchOption(orderBy = Some EmployeeOrderKeys.FullName, asc = false, limit = 50)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    let ids = result.results |> Seq.map (fun e -> e.Id) |> Seq.toArray
    Assert.That(ids[0], Is.EqualTo(8L))
    Assert.That(ids[7], Is.EqualTo(1L))

[<Test>]
let ``Order by Email ascending`` () =
    let repo = TestDb.createRepository ()

    let options =
        EmployeeSearchOption(orderBy = Some EmployeeOrderKeys.Email, asc = true, limit = 50)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    let ids = result.results |> Seq.map (fun e -> e.Id) |> Seq.toArray
    Assert.That(ids[0], Is.EqualTo(1L))

[<Test>]
let ``Order by Email descending`` () =
    let repo = TestDb.createRepository ()

    let options =
        EmployeeSearchOption(orderBy = Some EmployeeOrderKeys.Email, asc = false, limit = 50)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    let ids = result.results |> Seq.map (fun e -> e.Id) |> Seq.toArray
    Assert.That(ids[0], Is.EqualTo(7L))

[<Test>]
let ``Order by PhoneNumber ascending`` () =
    let repo = TestDb.createRepository ()

    let options =
        EmployeeSearchOption(orderBy = Some EmployeeOrderKeys.PhoneNumber, asc = true, limit = 50)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    let ids = result.results |> Seq.map (fun e -> e.Id) |> Seq.toArray
    Assert.That(ids[0], Is.EqualTo(1L))

[<Test>]
let ``Order by PhoneNumber descending`` () =
    let repo = TestDb.createRepository ()

    let options =
        EmployeeSearchOption(orderBy = Some EmployeeOrderKeys.PhoneNumber, asc = false, limit = 50)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    let ids = result.results |> Seq.map (fun e -> e.Id) |> Seq.toArray
    Assert.That(ids[0], Is.EqualTo(8L))

[<Test>]
let ``Order by RoleName ascending`` () =
    let repo = TestDb.createRepository ()

    let options =
        EmployeeSearchOption(orderBy = Some EmployeeOrderKeys.RoleName, asc = true, limit = 50)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    let ids = result.results |> Seq.map (fun e -> e.Id) |> Seq.toArray
    // SQLite sorts NULLs first for ASC, then alphabetically: admin, manager, user
    Assert.That(ids, Is.EqualTo([| 6L; 1L; 3L; 7L; 5L; 2L; 4L; 8L |] :> obj))

[<Test>]
let ``Order by RoleName descending`` () =
    let repo = TestDb.createRepository ()

    let options =
        EmployeeSearchOption(orderBy = Some EmployeeOrderKeys.RoleName, asc = false, limit = 50)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    let ids = result.results |> Seq.map (fun e -> e.Id) |> Seq.toArray
    // SQLite sorts NULLs last for DESC (NULL < any value, reversed by DESC), then reverse alphabetical: user, manager, admin
    Assert.That(ids, Is.EqualTo([| 8L; 4L; 2L; 5L; 7L; 3L; 1L; 6L |] :> obj))

[<Test>]
let ``Default order by EmployeeId ascending`` () =
    let repo = TestDb.createRepository ()
    let options = EmployeeSearchOption(orderBy = None, asc = true, limit = 50)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    let ids = result.results |> Seq.map (fun e -> e.Id) |> Seq.toArray
    Assert.That(ids, Is.EqualTo([| 1L; 2L; 3L; 4L; 5L; 6L; 7L; 8L |] :> obj))

[<Test>]
let ``Default order by EmployeeId descending`` () =
    let repo = TestDb.createRepository ()
    let options = EmployeeSearchOption(orderBy = None, asc = false, limit = 50)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    let ids = result.results |> Seq.map (fun e -> e.Id) |> Seq.toArray
    Assert.That(ids, Is.EqualTo([| 8L; 7L; 6L; 5L; 4L; 3L; 2L; 1L |] :> obj))

[<Test>]
let ``Stable sort uses then-by keys on tie`` () =
    let repo = TestDb.createRepository ()

    let options =
        EmployeeSearchOption(
            fullName = Some "GRACE SMITH",
            orderBy = Some EmployeeOrderKeys.FullName,
            asc = true,
            limit = 50
        )

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    let ids = result.results |> Seq.map (fun e -> e.Id) |> Seq.toArray
    Assert.That(ids, Is.EqualTo([| 7L; 8L |] :> obj))

[<Test>]
let ``Pagination returns first page`` () =
    let repo = TestDb.createRepository ()
    let options = EmployeeSearchOption(limit = 3, skip = 0, asc = true)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    Assert.That(result.results |> Seq.length, Is.EqualTo(3))
    Assert.That(result.total, Is.EqualTo(8L))
    Assert.That(result.nextCursor, Is.EqualTo(Some 3L))

[<Test>]
let ``Pagination returns second page`` () =
    let repo = TestDb.createRepository ()
    let options = EmployeeSearchOption(limit = 3, skip = 3, asc = true)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    Assert.That(result.results |> Seq.length, Is.EqualTo(3))
    let ids = result.results |> Seq.map (fun e -> e.Id) |> Seq.toArray
    Assert.That(ids, Is.EqualTo([| 4L; 5L; 6L |] :> obj))
    Assert.That(result.nextCursor, Is.EqualTo(Some 6L))

[<Test>]
let ``Cursor is None when all results fit in one page`` () =
    let repo = TestDb.createRepository ()
    let options = EmployeeSearchOption(limit = 50, skip = 0, asc = true)

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    Assert.That(result.nextCursor, Is.EqualTo(None))

[<Test>]
let ``Full pipeline with filter order pagination`` () =
    let repo = TestDb.createRepository ()

    let options =
        EmployeeSearchOption(
            fullName = Some "SMITH",
            orderBy = Some EmployeeOrderKeys.FullName,
            asc = true,
            limit = 1,
            skip = 0
        )

    let result =
        repo.SearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    Assert.That(result.total, Is.EqualTo(2L))
    Assert.That(result.results |> Seq.length, Is.EqualTo(1))
    let emp = result.results |> Seq.head
    Assert.That(emp.Id, Is.EqualTo(7L))
    Assert.That(result.nextCursor, Is.EqualTo(Some 1L))

[<Test>]
let ``Count with no filters returns total`` () =
    let repo = TestDb.createRepository ()
    let options = EmployeeSearchOption()

    let count =
        repo.CountSearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    Assert.That(count, Is.EqualTo(8))

[<Test>]
let ``Count with filter returns filtered total`` () =
    let repo = TestDb.createRepository ()
    let options = EmployeeSearchOption(fullName = Some "BROWN")

    let count =
        repo.CountSearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    Assert.That(count, Is.EqualTo(2))

[<Test>]
let ``Count with no matching returns zero`` () =
    let repo = TestDb.createRepository ()
    let options = EmployeeSearchOption(fullName = Some "NONEXISTENT")

    let count =
        repo.CountSearchAsync(options, CancellationToken.None) |> Async.RunSynchronously

    Assert.That(count, Is.EqualTo(0))

[<Test>]
let ``CancellationToken cancelled throws`` () =
    let repo = TestDb.createRepository ()
    let options = EmployeeSearchOption(limit = 50)
    use cts = new CancellationTokenSource()
    cts.Cancel()

    let ex =
        Assert.Throws<TaskCanceledException>(
            Action(fun () -> repo.SearchAsync(options, cts.Token) |> Async.RunSynchronously |> ignore)
        )

    Assert.That(ex, Is.Not.Null)
