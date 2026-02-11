# Research: .NET Core Traditional Authentication

**Branch**: `001-dotnet-traditional-auth` | **Date**: 2026-02-11

## Phase 0: Resolve Unknowns

### 1. JWT Token Strategy

**Decision**: HS256 symmetric signing with short-lived access tokens (15 minutes) and long-lived refresh tokens (7 days) stored in the database.

**Rationale**:
- HS256 is sufficient for a single-service architecture where the same service issues and validates tokens.
- Short access token lifetime limits exposure if a token is compromised.
- Refresh token rotation (new token on each refresh, old token invalidated) prevents replay attacks.
- Database-backed refresh tokens enable explicit revocation on logout.

**Alternatives considered**:
- RS256 asymmetric signing: More appropriate for multi-service architectures where different services validate tokens. Unnecessary complexity for a single auth service.
- Token blacklisting for access tokens: High-performance cost for short-lived tokens. Not justified given 15-minute expiry.

### 2. Password Hashing

**Decision**: BCrypt.Net-Next with work factor 12.

**Rationale**:
- BCrypt is the industry standard for password hashing with built-in salt generation.
- Work factor 12 provides approximately 250ms hash time on modern hardware, balancing security and performance.
- BCrypt.Net-Next is the actively maintained .NET implementation.

**Alternatives considered**:
- Argon2: Newer and memory-hard, but less ecosystem support in .NET. BCrypt is the constitution requirement.
- PBKDF2: Built into ASP.NET Core Identity but less resistant to GPU attacks than BCrypt.

### 3. Rate Limiting Approach

**Decision**: Custom middleware with in-memory tracking per IP address. 5 failed login attempts per 15-minute sliding window.

**Rationale**:
- .NET 8 includes built-in rate limiting middleware (`Microsoft.AspNetCore.RateLimiting`), but a custom implementation provides finer control over tracking failed login attempts specifically (not all requests).
- In-memory storage is sufficient for single-instance deployment. For multi-instance, a distributed cache (Redis) would be needed.

**Alternatives considered**:
- Built-in `RateLimiterMiddleware`: Works for general request rate limiting but doesn't easily track failed-login-specific counters.
- Redis-backed rate limiting: Necessary for horizontal scaling but adds infrastructure complexity. Can be added later.

### 4. Database Choice

**Decision**: PostgreSQL 15+ with Entity Framework Core 8 and Npgsql provider.

**Rationale**:
- PostgreSQL is a mature, production-grade RDBMS with excellent JSON support and performance.
- EF Core 8 provides LINQ-based queries, migrations, and change tracking.
- Npgsql is the official .NET provider for PostgreSQL, actively maintained.

### 5. Error Response Strategy

**Decision**: Consistent JSON envelope with `ProblemDetails` for HTTP errors and custom `ApiResponse<T>` wrapper for all responses.

**Rationale**:
- `ProblemDetails` is the RFC 7807 standard and ASP.NET Core convention.
- Custom `ApiResponse<T>` provides the `{ success, data, message, timestamp }` format required by the specification.
- `ErrorHandlingMiddleware` catches unhandled exceptions and converts to consistent error format.

### 6. Audit Logging Strategy

**Decision**: Database-persisted audit logs with structured Serilog console/file logging in parallel.

**Rationale**:
- Database audit logs enable querying and reporting through admin endpoints.
- Serilog JSON logging provides real-time observability and can feed into external log aggregation systems.
- Both mechanisms run in parallel — Serilog for operational monitoring, database for business audit trail.
