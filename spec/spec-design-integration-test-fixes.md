---
title: Fix Integration Test Failures
version: 1.0
date_created: 2026-06-23
tags: test, integration, moq
---

# Fix Integration Test Failures

## 1. Purpose & Scope

Document and resolve two pre-existing integration test failures in `Storczyk.Prodpol.Integrations`. Both failures occur in `EmployeeIntegrationTests.fs`.

## 2. Definitions

| Term | Definition |
|---|---|
| Moq | .NET mocking library used for creating test doubles |
| Tupled parameter | F# method signature `(key: int64 * entity: Employee)` — single parameter that is a tuple |
| Curried parameter | F# function style `key -> entity -> result` — multiple sequential parameters |

## 3. Failures

### FAILURE-001: Moq callback parameter count mismatch

**File:** `tests/Storczyk.Prodpol.Integrations/EmployeeIntegrationTests.fs:178`

**Error:** `System.ArgumentException : Invalid callback. Setup on method with 2 parameter(s) cannot invoke callback with different number of parameters (1).`

**Root cause:** The method `IEmployeesRepository.UpdateAsync` has a tupled F# signature `(key: int64 * entity: Employee) -> Async<unit>`. Moq sees the setup `m.UpdateAsync(It.IsAny<int64>(), It.IsAny<Employee>())` as **2 separate parameters**, but `.Returns(fun (_key: int64, _emp: Employee) -> ...)` provides a **single 2-tuple** (counted as 1 parameter).

**Fix:** Change the `.Returns` callback from tupled to curried syntax:

```fsharp
// Before (line 178):
.Returns(fun (_key: int64, _emp: Employee) -> async { return () })

// After:
.Returns(fun (_key: int64) (_emp: Employee) -> async { return () })
```

### FAILURE-002: GET test returns 500 InternalServerError

**File:** `tests/Storczyk.Prodpol.Integrations/EmployeeIntegrationTests.fs`

**Error:** `GET /api/data/employees/26 returns mocked employee` — expects 200 OK, gets 500 InternalServerError.

**Root cause:** Unknown. Hypothesis: one of the recent DI changes (`.AddUserValidator<EmployeeIdValidator>()`, `EmployeeUserManager` constructor rewrite) may have introduced a service resolution failure. Alternatively, it may be a pre-existing issue unrelated to current changes.

**Diagnosis steps:**
1. Roll back `EmployeeIdValidator` registration and `EmployeeUserManager` constructor changes temporarily
2. Run the GET test in isolation: `dotnet test --filter "GET /api/data/employees/26"`
3. If it passes: one of the DI changes broke it → bisect to find which
4. If it still fails: it's a pre-existing issue → investigate separately

## 4. Implementation Order

1. Fix FAILURE-001 (clear, single-line Moq callback change)
2. Diagnose FAILURE-002
3. Fix FAILURE-002 based on diagnosis

## 5. Verification

- `dotnet test tests/Storczyk.Prodpol.Tests/` — 33/33 must pass
- `dotnet test tests/Storczyk.Prodpol.Integrations/` — 5/5 must pass
- `dotnet build` — 0 errors
