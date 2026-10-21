# B.Well Clinics — Loyalty Program Backend

ASP.NET Core Web API + SQL Server + **Paymob** for the B.Well Clinics Loyalty Program.

## Solution structure

| Project | Role |
|---------|------|
| `BWell.Loyalty.Domain` | Entities & enums |
| `BWell.Loyalty.Application` | DTOs & service contracts |
| `BWell.Loyalty.Infrastructure` | EF Core, SQL Server, Paymob, business services |
| `BWell.Loyalty.Api` | HTTP controllers, Swagger, JWT |

## Prerequisites

- .NET 10 SDK
- SQL Server or LocalDB (`(localdb)\mssqllocaldb`)
- Paymob Egypt test/live account

## Configuration

Copy keys into a **gitignored** local file:

`BWell.Loyalty.Api/appsettings.Paymob.local.json`

```json
{
  "Paymob": {
    "ApiKey": "YOUR_API_KEY_JWT",
    "SecretKey": "egy_sk_test_...",
    "PublicKey": "egy_pk_test_...",
    "HmacSecret": "FROM_DASHBOARD",
    "IntegrationIds": [],
    "PaymentMethodNames": ["card"],
    "BaseUrl": "https://accept.paymob.com",
    "Currency": "EGP",
    "NotificationBaseUrl": "http://localhost:5068",
    "RedirectionUrl": "http://localhost:5173/checkout/result",
    "UseMockPayments": false
  }
}
```

Or use example template: `appsettings.Paymob.Example.json`

### Important Paymob fields

| Field | Notes |
|-------|--------|
| `SecretKey` / `PublicKey` | Intention API (`egy_sk_` / `egy_pk_`) |
| `ApiKey` | Classic JWT API key (utilities) |
| `IntegrationIds` | From Paymob → Settings → Payment Integrations (preferred). If empty, uses `PaymentMethodNames` (e.g. `card`) |
| `HmacSecret` | Required for secure webhooks in production |
| `NotificationBaseUrl` | Must be publicly reachable for Paymob callbacks (use ngrok locally) |

### Payment flow

1. `POST /api/subscriptions/checkout/complete-payment` → creates Paymob **Intention** → returns `clientSecret`, `publicKey`, `checkoutUrl`
2. Frontend redirects user to `checkoutUrl` (Unified Checkout)
3. Paymob calls `POST /api/payments/paymob/webhook?hmac=...` (source of truth)
4. Optional: `POST /checkout/confirm-payment` after redirect

## Run

```bash
cd backend
dotnet ef database update --project BWell.Loyalty.Infrastructure --startup-project BWell.Loyalty.Api
dotnet run --project BWell.Loyalty.Api --launch-profile http
```

Swagger: `http://localhost:5068/swagger`

## Default admin

- Email: `admin@bwell.local`
- Password: `Admin@123`

## Main API routes

| Method | Route | Auth | Purpose |
|--------|-------|------|---------|
| POST | `/api/auth/login` | Public | Admin login |
| GET | `/api/programs/pricing?planType=Individual\|Family` | Public | Pricing cards |
| GET | `/api/programs` | Public | All programs |
| POST/PUT/DELETE | `/api/programs` | Admin | Manage programs |
| POST | `/api/subscriptions/checkout/start` | Public | Steps 1–2 data |
| POST | `/api/subscriptions/checkout/complete-payment` | Public | Create Paymob intention |
| POST | `/api/subscriptions/checkout/confirm-payment` | Public | Optional status check |
| POST | `/api/subscriptions/checkout/mark-failed` | Public | Marks payment failed |
| GET | `/api/subscriptions/payment-config` | Public | Public key + provider flags |
| GET | `/api/subscriptions/{id}/order-summary` | Public | Success/Failed screen |
| POST | `/api/payments/paymob/webhook` | HMAC | Payment events |
| GET | `/api/patients` | Admin | Patients list |
| GET | `/api/analytics/dashboard` | Admin | Overview KPIs |

## Security layers

- JWT Bearer auth + `AdminOnly` policy on admin endpoints
- FluentValidation on login / checkout / programs / visits
- Rate limiting: global, login, payments
- Security headers + exception middleware
- CORS allow-list
- PBKDF2 password hashing
- Paymob webhook HMAC verification (when `HmacSecret` is set)

## Business rules

- Individual vs Family pricing and benefits
- Family max 4 members
- Checkout creates patient + pending subscription, then Paymob payment activates it
- Visit discounts from program service rules (1st / 2nd / additional)
- Soft-delete; cannot delete programs with active subscriptions
