# Quickstart: .NET Core Traditional Authentication

**Branch**: `001-dotnet-traditional-auth` | **Date**: 2026-02-11

## Prerequisites

- .NET 8.0 SDK
- PostgreSQL 15+ (or Docker)
- Git

## 5-Minute Setup

### 1. Clone and Build

```bash
git clone <repository-url>
cd authentication-server-dotnet
dotnet restore
dotnet build
```

### 2. Start PostgreSQL (Docker)

```bash
docker-compose up -d postgres
```

Or configure an existing PostgreSQL instance in `Auth.Api/appsettings.Development.json`.

### 3. Configure Settings

Edit `Auth.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=authdb;Username=postgres;Password=password"
  },
  "JwtSettings": {
    "Secret": "your-super-secret-jwt-key-min-32-chars-long",
    "Issuer": "auth-api",
    "Audience": "auth-client",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "BcryptSettings": {
    "WorkFactor": 12
  }
}
```

### 4. Apply Migrations

```bash
cd Auth.Api
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run --project Auth.Api
```

The API is now available at `https://localhost:5001` (or `http://localhost:5000`).

### 6. Open Swagger UI

Navigate to `https://localhost:5001/api/docs` to explore the API documentation.

## Quick Test with curl

### Register a User

```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test@1234",
    "firstName": "Test",
    "lastName": "User"
  }'
```

### Login

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test@1234"
  }'
```

### Access Protected Endpoint

```bash
curl -X GET https://localhost:5001/api/users/me \
  -H "Authorization: Bearer <access_token_from_login>"
```

### Refresh Token

```bash
curl -X POST https://localhost:5001/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "<refresh_token_from_login>"
  }'
```

### Health Check

```bash
curl https://localhost:5001/health
curl https://localhost:5001/ready
```

## Running Tests

```bash
dotnet test
```

## Docker Setup

```bash
docker-compose up -d
```

This starts both the authentication API and PostgreSQL.

## Seed Data

On first run, the database is seeded with:

- **Roles**: Admin, User, Guest
- **Admin user**: admin@example.com / Admin@1234 (change in production)
