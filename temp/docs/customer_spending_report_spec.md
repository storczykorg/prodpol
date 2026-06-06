# Technical Specification: Customer Spending Report View

## 1. Title & Overview
**Project**: Prodpol E-commerce CMS  
**Feature**: Customer Spending Report  
**Status**: Draft (Technical Specification)  
**Author**: Senior Software Architect

### Overview
This document specifies the creation of a new database view, `prodpol.customer_spending_report`, designed to aggregate customer purchasing behavior. This report is essential for business intelligence, allowing the system to identify high-value customers and track overall sales performance per user.

---

## 2. Architecture Diagram
The following diagram illustrates the data flow from the raw database tables through the view into the API layer.

```mermaid
graph TD
    subgraph Database Layer (PostgreSQL)
        T1[prodpol.customers] --> V[customer_spending_report]
        T2[prodpol.orders] --> V
        T3[prodpol.order_products] --> V
        T4[prodpol.delivery_methods] --> V
    end

    subgraph Backend Layer (F#)
        V --> R[PgReportRepository]
        R --> S[ReportService]
    end

    subgraph API Layer (ASP.NET Core)
        S --> C[ReportsController]
    end
```

---

## 3. Data Model

### SQL View Definition
The view will be implemented in `lib/Storczyk.Database/Postgres/views/`. It leverages the existing calculation logic found in `prodpol.compute_order_total`.

**Schema**: `prodpol`  
**Name**: `customer_spending_report`

```sql
CREATE OR REPLACE VIEW prodpol.customer_spending_report AS
WITH order_totals AS (
    SELECT 
        o.customer_id,
        o.order_id,
        (COALESCE(SUM(op.total_cost), 0::money) + COALESCE(dm.base_cost, 0::money)) as total_order_value
    FROM prodpol.orders o
    LEFT JOIN prodpol.order_products op ON o.order_id = op.order_id
    LEFT JOIN prodpol.delivery_methods dm ON o.delivery_method = dm.delivery_id
    GROUP BY o.customer_id, o.order_id, dm.base_cost
)
SELECT 
    c.customer_id,
    c.full_name,
    c.email,
    COUNT(ot.order_id) as total_orders,
    SUM(ot.total_order_value) as total_spent,
    AVG(ot.total_order_value) as average_order_value,
    MAX(ot.total_order_value) as most_expensive_order,
    (SELECT MAX(created_at) FROM prodpol.orders WHERE customer_id = c.customer_id) as last_order_date
FROM prodpol.customers c
JOIN order_totals ot ON c.customer_id = ot.customer_id
GROUP BY c.customer_id, c.full_name, c.email;
```

---

## 4. API Specification

### Endpoint: `GET /api/reports/customer-spending`
Returns a paginated or full list of customer spending metrics.

**Response Body (JSON)**:
```json
[
  {
    "customerId": 101,
    "fullName": "John Doe",
    "email": "john.doe@example.com",
    "totalOrders": 5,
    "totalSpent": 1250.50,
    "averageOrderValue": 250.10,
    "mostExpensiveOrder": 500.00,
    "lastOrderDate": "2026-06-01T14:00:00Z"
  }
]
```

---

## 5. Implementation Details

### Backend (F#)
1.  **Model**: Create a new record type `CustomerSpendingReport` in `Storczyk.Prodpol.Core/Models/`.
2.  **Repository**: Implement `ICustomerReportRepository` in `Storczyk.Prodpol.Core/Data/` and its PostgreSQL implementation in `Storczyk.Prodpol.Core/Services/PgReportRepository.fs`.
3.  **Dapper Integration**: Use Dapper to query the view: 
    `SELECT * FROM prodpol.customer_spending_report ORDER BY total_spent DESC`.

### Database (DbUp)
The view must be added to `lib/Storczyk.Database/Postgres/views/` as an idempotent script. As per `GEMINI.md`, use `CREATE OR REPLACE VIEW`.

### References
*   *Project Standards*: Root `GEMINI.md` (F# Records, AsyncResult).
*   *Existing Logic*: `lib/Storczyk.Database/Postgres/procedures/0060_compute_order_total.sql`.
*   *PostgreSQL Documentation*: [Views](https://www.postgresql.org/docs/current/sql-createview.html).
