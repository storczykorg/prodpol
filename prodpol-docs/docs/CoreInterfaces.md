# Core Interface Types

This page documents the core interface contracts defined in
`lib/Storczyk.Prodpol.Core` used across the application for repositories and
infrastructure.

## IReadRepository<'TKey,'TValue>

Location: [lib/Storczyk.Prodpol.Core/Data/IRepository.fs](lib/Storczyk.Prodpol.Core/Data/IRepository.fs#L1-L60)

Purpose: read-only repository abstraction.

Members:
- `GetAllAsync(token: CancellationToken) : Async<Result<AsyncSeq<'TValue>, DatabaseError>>` — returns an async sequence of values wrapped in `Result`.
- `GetByIdAsync(key: 'TKey) : Async<Result<'TValue, DatabaseError>>` — fetch single value.
- `CountAsync(token: CancellationToken) : Async<Result<int64, DatabaseError>>` — count items.

Usage: Implement for repositories that require streaming reads (e.g., DB selects).

## IRepository<'TKey,'TValue>

Location: [lib/Storczyk.Prodpol.Core/Data/IRepository.fs](lib/Storczyk.Prodpol.Core/Data/IRepository.fs#L1-L60)

Purpose: read-write repository interface that inherits `IReadRepository`.

Additional Members:
- `AddAsync(entity: 'TValue)` — produce `Async<Result<unit, DatabaseError>>`.
- `UpdateAsync(key, entity)` — update by key.
- `DeleteAsync(key)` — delete by key.

Usage: Primary write-facing contract for persistence implementations.

## IEmployeesRepository, IEmployeesReadRepository

Locations:
- [lib/Storczyk.Prodpol.Core/Data/IRepository.fs](lib/Storczyk.Prodpol.Core/Data/IRepository.fs#L1-L60)

These are project-specific aliases:
- `IEmployeesRepository` = `IRepository<int64, Employee>`
- `IEmployeesReadRepository` = `IReadRepository<int64, EmployeeRead>`

They express domain intent and simplify DI registration and mocking.

## IEmployeeSearchRepository

Location: [lib/Storczyk.Prodpol.Core/Data/IRepository.fs](lib/Storczyk.Prodpol.Core/Data/IRepository.fs#L60-L100)

Members:
- `SearchAsync(options: EmployeeSearchOption, token: CancellationToken) : Async<Result<EmployeeSearchResult, DatabaseError>>`

Purpose: specialized read operation for complex search queries returning a structured result type.

## IDictionaryRepository<'T> and IEmployeeRoleRepository

Location: [lib/Storczyk.Prodpol.Core/Data/IRepository.fs](lib/Storczyk.Prodpol.Core/Data/IRepository.fs#L1-L100)

- `IDictionaryRepository<'T>` is `IRepository<string,'T>` used for keyed dictionaries.
- `IEmployeeRoleRepository` = `IDictionaryRepository<EmployeeRole>` — repository for employee roles.

## ISnowflakeGenerator

Location: [lib/Storczyk.Prodpol.Core/Services/SnowflakeGenerator.fs](lib/Storczyk.Prodpol.Core/Services/SnowflakeGenerator.fs#L1-L120)

Purpose: provide unique 64-bit identifiers based on a custom epoch and instance id.

Members:
- `GetSnowflake(time: DateTime) : int64`
- `GetSnowflake() : int64` — convenience for current time.

Implementation: see `SnowflakeGenerator` in the same file.

## Notes and conventions

- All async operations in Core return `Async<Result<_, DatabaseError>>` so callers must explicitly handle both async and error flows (see `AsyncResult` utilities).
- Interface aliases (like `IEmployeesRepository`) are used for DI and to make types self-describing.
- For mocking in tests, implement the interface with `Async` returning `Ok / Error` as needed.
