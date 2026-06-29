# DineFlow ERD Modules

Use these smaller Mermaid ERDs when the full diagram is too dense:

- `01-auth-tables.mmd` - users, areas, dining tables, table sessions.
- `02-menu-pricing.mmd` - menu catalog, choices, sales channels, channel prices.
- `03-ordering-requests.mmd` - customer/staff orders and service requests.
- `04-billing-payments.mmd` - bills, bill details, adjustments, and payments.

Tables owned by another module are intentionally left as name-only nodes when they are shown just to clarify foreign-key links.

The full system ERD remains at `../dineflow-current-erd.mmd`.

Related flow document: `../system-flow-sales-channel-bill.md`.
