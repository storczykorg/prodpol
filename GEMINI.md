# Project Instructions - Prodpol

This file serves as the core instruction guide and architectural reference for the Prodpol project. All developers and AI assistants must follow these guidelines and conventions to maintain consistency, type safety, and clean development flows.

---

## Table of Contents
1. [Project Overview](#1-project-overview)
2. [Workspace Architecture](#2-workspace-architecture)
3. [Building and Running](#3-building-running-and-testing)
4. [Development Conventions & Idioms](#4-development-conventions--idioms)
5. [Database Migrations (DbUp)](#5-database-migrations-dbup)
6. [Testing Practices](#6-testing-practices)
7. [Frontend Guidelines](#7-frontend-guidelines)

---

## 1. Project Overview
**Prodpol** is a modern e-commerce CMS in early development. It is designed to run in a distributed, modern container environment using **.NET Aspire** as the system-wide orchestrator, featuring a mixed **F# / C#** backend, a **PostgreSQL 18** database, and a **Vue 3 / Vite / TypeScript** frontend.

---

## 2. Workspace Architecture
The repository is organized into distinct logical tiers across F# / C# backend libraries, frontend components, background processing workers, and documentation.

```
/ (Root)
├── lib/
│   ├── Storczyk.Database/          - C# migration library using DbUp to upgrade PostgreSQL schema and seed data
│   └── Storczyk.Prodpol.Core/       - F# business domain layer containing Models, Interfaces, and AsyncResult Utils
├── src/
│   ├── Storczyk.AppHost/           - .NET Aspire C# AppHost project orchestrating and running services in Docker
│   ├── Storczyk.Frontend/          - Vue 3 (SFC) single-page application built on Vite & TypeScript with Tailwind CSS v4
│   ├── Storczyk.Prodpol/           - ASP.NET Core F# Web API project serving backend endpoints
│   └── Storczyk.Worker/            - F# background worker service for message processing/scheduled tasks
├── tests/
│   ├── Storczyk.Prodpol.Tests/      - Backend controller and utility unit tests (F# / NUnit / Moq)
│   └── Storczyk.Prodpol.Integrations/ - API Integration tests spinning up an in-memory ASP.NET Core TestHost
└── prodpol-docs/                   - Vitepress-based documentation site
```

---

## 3. Building, Running, and Testing

### Prerequisites
* **.NET 10.0 SDK or higher** (Roll-forward configured via `global.json`)
* **Node.js v25 or higher**
* **Docker Desktop / Docker Daemon** (Required by .NET Aspire for launching containerized PostgreSQL 18)

### Commands Reference

#### Full Environment (Aspire Orchestrator)
To start the entire environment (Postgres, PgAdmin, Backend API, Worker, and Vue Frontend):
```bash
dotnet run --project src/Storczyk.AppHost
```

#### Backend Commands (.NET)
* **Build Solution:**
  ```bash
  dotnet build
  ```
* **Run Tests:**
  ```bash
  dotnet test
  ```
* **Format F# Code:**
  Format all F# files using the standard `fantomas` tool version 7.0.5:
  ```bash
  dotnet tool restore
  dotnet fantomas .
  ```

#### Frontend Commands (Vite + Vue)
Execute these commands from the root workspace or directly within the `src/Storczyk.Frontend` directory:
* **Install Dependencies:** `npm install`
* **Development Server:** `npm run dev -w src/Storczyk.Frontend`
* **Build Production Asset:** `npm run build -w src/Storczyk.Frontend`
* **Linter (ESLint with auto-fix):** `npm run lint -w src/Storczyk.Frontend`
* **Dead Code Analysis (Knip):** `npm run knip -w src/Storczyk.Frontend`

#### Documentation Commands (Vitepress)
Execute these from the `prodpol-docs/` directory:
* **Install Dependencies:** `npm install`
* **Development Server:** `npm run dev`
* **Build Docs:** `npm run build`

---

## 4. Development Conventions & Idioms

### F# Architectural Design & Idioms
* **Immutability and Expressions:** Always prefer immutable records and discriminated unions for data transfer and business logic representation. Use expression-based styling.
* **Asynchronous Flows:** Always use `async { ... }` computation expressions for non-blocking I/O.
* **Error Handling (`AsyncResult`):** Prefer returning `AsyncResult<T, DatabaseError>` over throwing exceptions or returning nulls. Pipe operations elegantly using standard pattern matches or combinators located under `Storczyk.Prodpol.Core.Utils.AsyncResult`.
* **JSON Custom Converters:** Custom JSON converters (`JsonFSharpConverter`) from `System.Text.Json.Serialization` are applied in `Program.fs` to enable seamless serialization of F# `Option`, Discriminated Unions, and Records.
* **Model Binding:** F# options are supported in API controllers via the custom `FSharpOptionModelBinderProvider`.

### Code Formatting
* Code formatting is enforced using **Fantomas** (`dotnet fantomas`). Ensure you run formatting before submitting any PRs or staging commits.

---

## 5. Database Migrations (DbUp)
Database schema and tables are managed programmatically inside `lib/Storczyk.Database` via `DbUp`.

* **Execution Flow:** Database migrations are embedded as assembly resources: `<EmbeddedResource Include="Postgres\**\*.sql"/>`.
* **Script Run Groups:** Migrations are loaded hierarchically by subfolder and ordered using specific DB run groups (Schema, Types, Utils, Dictionaries, Tables, Checks, Foreign Keys, Indices, Views, Procedures, Seed).
* **Idempotency Rule:**
  Because migrations are run with `ScriptType.RunAlways`, **every SQL script must be idempotent** and safe to run multiple times without causing runtime exceptions or data loss.
  * Use `CREATE TABLE IF NOT EXISTS ...`
  * Use `CREATE OR REPLACE FUNCTION ...`
  * Add conditional checks where appropriate (e.g. column additions or schema adjustments).
* **Seed Data:** Sibling seed SQL scripts must reside in `Postgres/seed/` and are executed in transactions by `PostgresSeedUpgrader`.

---

## 6. Testing Practices

### Unit Tests (`tests/Storczyk.Prodpol.Tests`)
* Powered by **NUnit** and **Moq**.
* **Naming Convention:** Test functions must use double-backtick notation for human-readable test names.
  ```fsharp
  [<Test>]
  let ``Search returns Ok with results`` () =
      // Arrange, Act, Assert flow
  ```
* Ensure you fully mock repository layers and dependencies to make tests fast, isolated, and deterministic.

### Integration Tests (`tests/Storczyk.Prodpol.Integrations`)
* Powered by ASP.NET Core TestHost.
* Tests boot up an in-memory/test-host instance using `ProdpolServer().BuildTest([||], configureServices)`.
* Mock repository implementations where real database interactions are undesirable, and use `getClient server` to issue real HTTP requests against the in-memory endpoint.

---

## 7. Frontend Guidelines
* **Framework:** Vue 3 using the composition API with the `<script setup lang="ts">` pattern.
* **CSS & Design:** Tailwind CSS v4 and DaisyUI v5 are utilized for rich styling and component building.
* **State & Data Caching:** Use Pinia and `@pinia/colada` for caching and data fetch querying.
* Ensure all frontend changes pass `npm run lint` and `npm run knip` checks.
