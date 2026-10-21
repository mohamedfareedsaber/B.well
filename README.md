# B.Well Loyalty

B.Well Loyalty is a membership and loyalty platform for B.Well Clinics. It includes a React patient experience and an ASP.NET Core API for programs, subscriptions, patient portals, visits, and benefit redemptions.

## Project structure

- `frontend/` — React, TypeScript, and Vite application.
- `backend/` — .NET solution containing the API, application, domain, and infrastructure projects.
- `design-refs/` — UI design reference images.

## Prerequisites

- Node.js and npm
- .NET 10 SDK
- SQL Server or LocalDB

Paymob credentials are required to use live payment processing. The backend can also be configured for mock payments during local development.

## Run locally

### Backend

Configure the database connection and JWT settings in `backend/BWell.Loyalty.Api/appsettings.Development.json`. For Paymob, use the gitignored `appsettings.Paymob.local.json` file and the `appsettings.Paymob.Example.json` template.

From the repository root:

```powershell
dotnet ef database update --project backend/BWell.Loyalty.Infrastructure --startup-project backend/BWell.Loyalty.Api
dotnet run --project backend/BWell.Loyalty.Api --launch-profile http
```

The API Swagger UI is available at `http://localhost:5068/swagger`.

### Frontend

In a second terminal:

```powershell
cd frontend
npm install
npm run dev
```

Configure the frontend API base URL in the local environment file as needed.

## Main features

- Browse individual and family membership plans.
- Start a subscription checkout and process payment through Paymob.
- View membership, family members, benefits, visits, and estimated savings in the patient portal.
- Submit and track requests to redeem active membership benefits.
- Manage loyalty programs and patients through the admin area.

For backend API details and payment configuration, see [backend/README.md](backend/README.md).
