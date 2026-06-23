namespace Storczyk.Prodpol.Data.Services.Identity

open LinqToDB
open Storczyk.Prodpol.Core.Models

type IIdentityDatabase =
    interface
        inherit IDataContext
        abstract member EmployeeRead: ITable<EmployeeRead>
    end

type IdentityDatabase(options: DataOptions) =
    inherit LinqToDB.DataContext(options)

    new() = IdentityDatabase(DataOptions())

    interface IIdentityDatabase with
        member this.EmployeeRead = this.GetTable<EmployeeRead>()
