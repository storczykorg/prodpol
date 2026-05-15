# Prodpol
> Software is still in early development. Everything can change and will break in weird ways

Prodpol is an e-commerce CMS that targets modern technologies and solutions.
Its main purpose is to provide easy to manage application that works in a distributed environment and a template for simmilar solutions.

## Modules
- `src/Storczyk.AppHost` - An Application Orchestrator that manages lifetime of other modules
- `src/Storczyk.Frontend` - HTML App based on Vite + Vue
- `src/Storczyk.Prodpol` - API server written in ASP.NET Core
- `src/Storczyk.Worker` - Background task processor

## Stack
- **Postgres 18** - main database
- *TBD* Kafka - message broker for background tasks like image processing and raport generation
- *TBD* Redis - data cache

## Requirements
- Node v25 or higher
- .NET 10 or higher

## Licence
Prodpol and its modules are licenced on [MIT license](LICENCE.md)