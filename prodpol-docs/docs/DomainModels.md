# Domain Models

Location: [lib/Storczyk.Prodpol.Core/Models/](lib/Storczyk.Prodpol.Core/Models/)

Domain models are `[<Serializable; CLIMutable; ProdpolModel>]` records mapped to
PostgreSQL tables via LinqToDB `[<Table>]` / `[<Column>]` attributes.

## Conventions

- **`[<CLIMutable>]`** — allows Dapper / LinqToDB to materialize records via a
  parameterless constructor + property setters.
- **`[<ProdpolModel>]`** — marker attribute used by Dapper type-map registration
  at startup (`SqlMappings.fs`).
- **`Read` suffix** — maps to a **view** (not the base table) and includes
  **generated** or **computed** columns (e.g. `full_name`, `lowest_price`,
  `state_name`). Read variants are standalone records (duplicate base fields),
  not inheritance.
- **Option fields** — nullable DB columns are modelled as `'T option`.
- **Search types** — each searchable model has a companion `*OrderKeys` enum,
  `*SearchOption` class, and `*SearchResult` class.

## Product

| Model | DB Source | Key |
|---|---|---|
| `Product` | `prodpol.products` | `Id: int64` |
| `ProductRead` | `prodpol.products_omnibus` | `Id: int64` |

`ProductRead` extends `Product` with pricing analytics from a view:

| Column | Type | Description |
|---|---|---|
| `LowestPrice` | `decimal option` | Lowest price in last 30 days |
| `LowestPriceTo` | `DateTime option` | Start of lowest-price interval |
| `LowestPriceFrom` | `DateTime option` | End of lowest-price interval |

### Product search

```fsharp
type ProductOrderKeys = ProductId | Name | Price | CreatedAt | AvailableAmount

type ProductSearchOption =
    member name: string option
    member priceMin: decimal option
    member priceMax: decimal option
    member unitType: int option
    member limit: int              // default 20
    member skip: int               // default 0
    member orderBy: ProductOrderKeys option
    member asc: bool               // default false

type ProductSearchResult = inherit PersonSearchResult<ProductRead>
```

## ProductDescription

| Model | DB Source | Key |
|---|---|---|
| `ProductDescription` | `prodpol.product_descriptions` | `(ProductId: int64, LanguageCode: string option)` |

Composite primary key `(product_id, language_code)`. Each row is a localized
product description with a rich-text `body` and an `isPublic` visibility flag.

## Customer

| Model | DB Source | Key |
|---|---|---|
| `Customer` | `prodpol.customers` | `Id: int64` |
| `CustomerRead` | `prodpol.customers_with_roles` | `Id: int64` |
| `CustomerRole` | `prodpol.customer_roles` | `Id: int` |

`Customer` maps to the base `customers` table (no generated columns).
`CustomerRead` maps to `customers_with_roles` view and adds:

| Column | Type | Description |
|---|---|---|
| `FullName` | `string` | Generated `name_first \|\| ' ' \|\| name_last` |
| `NormalizedEmail` | `string` | Generated `lower(email)` |
| `NormalizedName` | `string` | Generated `lower(name_first \|\| ' ' \|\| name_last)` |
| `RoleId` | `int option` | From joined `customer_roles` |
| `RoleDisplayName` | `string option` | From joined `customer_roles` |
| `RoleName` | `string option` | From joined `customer_roles` |

### Customer search

```fsharp
type CustomerOrderKeys = CustomerId | FullName | Email | PhoneNumber

type CustomerSearchOption = inherit PersonSearchOption<CustomerOrderKeys>

type CustomerSearchResult = inherit PersonSearchResult<CustomerRead>
```

Filters inherited from `PersonSearchOption`: `fullName`, `email`, `phoneNumber`,
`roleNames`, `limit`, `skip`, `orderBy`, `asc`.

## Order

| Model | DB Source | Key |
|---|---|---|
| `Order` | `prodpol.orders` | `Id: int64` |
| `OrderRead` | `prodpol.order_joined` | `Id: int64` |
| `OrderTotalDetails` | (composite type) | — |
| `OrderDetails` | `prodpol.order_details` | `OrderId: int64` |
| `OrderProduct` | `prodpol.order_products` | `OrderProductId: int64` |

`Order.Total` uses `OrderTotalDetails`, a record mapping the PostgreSQL
composite type `prodpol.order_total_details(orders_total, delivery_fee, items_total)`.

`OrderRead` maps to `order_joined` view and adds:

| Column | Type | Description |
|---|---|---|
| `StateDisplayName` | `string` | Display name from `order_states` |
| `StateName` | `string` | System name from `order_states` |

`OrderDetails` holds shipping / invoice address per order.
`OrderProduct` holds individual line items per order.

### Order search

```fsharp
type OrderOrderKeys = OrderId | State | Total | CustomerId | EmployeeId

type OrderSearchOption =
    member employeeIds: int64 array option
    member customerIds: int64 array option
    member createdFrom: DateTime option
    member createdTo: DateTime option
    member stateIds: int array option
    member stateNames: string array option
    member limit: int              // default 20
    member skip: int               // default 0
    member orderBy: OrderOrderKeys option
    member asc: bool               // default false

type OrderSearchResult = inherit PersonSearchResult<OrderRead>
```

## Shared search types

Defined in `Models/EmployeeSearch.fs` and reused by all model search types:

| Type | Purpose |
|---|---|
| `PersonSearchOption<'TOrder>` | Base class with pagination (`limit`, `skip`), ordering (`orderBy`, `asc`), and optional person-specific filters |
| `PersonSearchResult<'T>` | Generic result with `total: int64`, `nextCursor: int64 option`, `results: 'T seq` |

## Utility helpers

Each `*OrderKeys` enum has a companion module with a `GetSqlName` function that
maps enum values to their SQL column name strings:

| Module | Example |
|---|---|
| `ProductUtilHelpers` | `GetSqlName ProductOrderKeys.Price` → `"product_price"` |
| `CustomerUtilHelpers` | `GetSqlName CustomerOrderKeys.Email` → `"customer_email"` |
| `OrderUtilHelpers` | `GetSqlName OrderOrderKeys.State` → `"order_state"` |

## Repository interfaces

Location: [lib/Storczyk.Prodpol.Data/Data/IRepository.fs](lib/Storczyk.Prodpol.Data/Data/IRepository.fs)

| Interface | Alias For | Search Interface |
|---|---|---|
| `IProductReadRepository` | `IReadRepository<int64, ProductRead>` | `IProductSearchRepository` |
| `IProductDescriptionRepository` | `IRepository<int64 * string option, ProductDescription>` | — |
| `ICustomerRepository` | `IRepository<int64, Customer>` | — |
| `ICustomerReadRepository` | `IReadRepository<int64, CustomerRead>` | `ICustomerSearchRepository` |
| `ICustomerRoleRepository` | `IDictionaryRepository<CustomerRole>` | — |
| `IOrderRepository` | `IRepository<int64, Order>` | — |
| `IOrderReadRepository` | `IReadRepository<int64, OrderRead>` | `IOrderSearchRepository` |
| `IOrderDetailsRepository` | `IRepository<int64, OrderDetails>` | — |
| `IOrderProductRepository` | `IRepository<int64, OrderProduct>` | — |
