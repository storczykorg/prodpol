namespace Storczyk.Prodpol.Core.Models

open LinqToDB.Mapping
open Storczyk.Prodpol.Core.Utils

[<Table("prodpol.employee_roles")>]
type EmployeeRole() =
    [<Column("role_id")>]
    [<Identity>]
    member val Id = -1 with get, set

    [<Column("display_name")>]
    member val DisplayName = "" with get, set

    [<Column("role_name")>]
    member val RoleName = "" with get, set

    override this.ToString() = Json.readableJson this

[<Table("prodpol.employee_roles")>]
type EmployeeRoleRead() =
    inherit EmployeeRole()

    [<Column("employees_count")>]
    member val EmployeesCount = 0 with get, set

    override this.ToString() = Json.readableJson this
