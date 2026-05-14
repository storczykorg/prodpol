# Prodpol
> Software is still in early development. Everything can change and will break in weird ways

Prodpol is an e-commerce CMS that targets modern technologies and solutions. 

## Modules
- `src/Storczyk.Frontend` - Html5 App
- `src/Storczyk.Prodpol` - API server
- `src/Storczyk.Worker` - Background task processor

## Stack
- **Postgres SQL** - main database
- *TBD* Kafka - message broker for background tasks like image processing and raport generation
- *TBD* Redis - data cache

## Requirements
- Node v25 or higher
- .NET 10 or higher
- Postgres 18

## Licence
Prodpol and it's modules are licenced on [MIT license](LICENCE.md)