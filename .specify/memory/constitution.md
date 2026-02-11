<!--
  Sync Impact Report
  ===================
  Version change: 0.0.0 → 1.0.0
  Modified principles: All (initial creation from template)
  Added sections: 10 Core Principles, Security Requirements, Performance Standards, Development Workflow, Governance
  Removed sections: None
  Templates requiring updates:
    ✅ plan-template.md (Constitution Check section aligns with principles)
    ✅ spec-template.md (requirements alignment verified)
    ✅ tasks-template.md (task categorization reflects principle-driven types)
    ✅ checklist-template.md (checklist categories align)
  Follow-up TODOs: None
-->

# .NET Core Traditional Authentication Constitution

## Core Principles

### I. Code Quality Standards

All source code MUST target .NET 8.0 LTS. StyleCop analyzers MUST be configured with zero warnings tolerated. An `.editorconfig` file MUST enforce consistent formatting across the solution. Minimum 80% test coverage is required for all production code. No `TODO` comments are permitted without a corresponding issue reference. All changes MUST go through code review before merging. Nullable reference types MUST be enabled project-wide. Async/await best practices MUST be followed — no blocking calls (`.Result`, `.Wait()`) on async code paths.

### II. Security Requirements (NON-NEGOTIABLE)

OWASP Top 10 compliance is mandatory for all features. No hardcoded secrets or credentials are permitted in source code — all secrets MUST come from configuration or environment variables. All passwords MUST be hashed with BCrypt.Net-Next using a work factor of 12 or higher. SQL injection MUST be prevented through EF Core parameterized queries exclusively — no raw SQL without parameterization. XSS protection MUST be enabled. CSRF protection MUST be applied for all state-changing operations. Rate limiting middleware MUST be active on authentication endpoints (5 failed attempts per 15-minute window). JWT Bearer authentication MUST be used with HS256 signing and short-lived access tokens (15-minute expiry). Token expiration MUST be enforced — expired tokens MUST be rejected. HTTPS redirection MUST be enabled in production. Security headers middleware MUST be configured.

### III. Testing Standards

Minimum 80% code coverage MUST be maintained. xUnit MUST be used for all unit testing. `WebApplicationFactory` MUST be used for integration testing. Moq MUST be used for mocking dependencies. Testcontainers MUST be used for test database provisioning. Unit tests MUST exist for all service classes. Integration tests MUST cover all API endpoints. Security-specific test cases MUST be included (authentication bypass, authorization escalation, token tampering, rate limiting).

### IV. API Design

RESTful conventions MUST be strictly followed for all endpoints. A consistent response format MUST be used: `{ success, data, message, timestamp }` for success and `{ success, error: { code, message, details }, timestamp }` for errors. OpenAPI 3.0 documentation MUST be generated via Swashbuckle. Swagger UI MUST be available at `/api/docs`. API versioning strategy MUST be defined and documented. Proper HTTP status codes MUST be returned for all responses. FluentValidation MUST be used for all request validation.

### V. ASP.NET Core Standards

Controllers (not Minimal APIs) MUST be used for all endpoints. Dependency injection MUST be performed via constructor injection exclusively. The `[ApiController]` attribute MUST be applied to all controllers. The `[Authorize]` attribute MUST protect all authenticated endpoints. The Options pattern MUST be used for configuration binding. `ILogger<T>` MUST be used for all logging. `ProblemDetails` MUST be used for error responses.

### VI. Documentation

A README with a 5-minute quick start guide MUST be maintained. XML documentation comments MUST be present on all public APIs. Architecture documentation MUST describe the clean architecture layers and their responsibilities. API examples with curl commands MUST be provided for all endpoints. `appsettings.json` structure MUST be documented with all configuration keys explained. A deployment guide MUST be included. Docker setup MUST be documented with `Dockerfile` and `docker-compose.yml`. A troubleshooting section MUST address common issues.

### VII. Performance

API response time MUST be under 200ms at the 95th percentile. Database connection pooling MUST be configured. EF Core queries MUST use `AsNoTracking()` for read-only operations. `IMemoryCache` MUST be used for caching roles and permissions. Pagination MUST be implemented for all list endpoints. N+1 query problems MUST be detected and eliminated.

### VIII. Reliability

Graceful error handling MUST be implemented via exception filters and middleware. Serilog MUST be configured for structured logging in JSON format. Health check endpoints MUST be available at `/health` and `/ready`. Database migration on startup MUST be supported (configurable). Graceful shutdown MUST be handled. Transaction management MUST be used for multi-step database operations.

### IX. Development Workflow

Feature branches MUST be created from `main`. Pull requests MUST be required before merging to `main`. All automated tests MUST pass before a PR can be merged. Commit messages MUST follow conventional commits format. Environment-specific `appsettings.{Environment}.json` files MUST be used for configuration.

### X. C# / .NET Conventions

Nullable reference types MUST be enabled in all projects. Record types SHOULD be used for DTOs. Pattern matching SHOULD be used where it improves readability. LINQ MUST be used for collection queries. AutoMapper MUST be used for entity-to-DTO mapping. Extension methods SHOULD be used to keep code clean and composable.

## Security Requirements

All authentication and authorization features are subject to Principle II (Security Requirements). In addition:

- Refresh tokens MUST implement rotation — a new refresh token is issued on each refresh, and the old token is invalidated.
- Audit logging MUST record all security-relevant events (login, logout, password change, failed attempts) with IP address, user agent, timestamp, and success status.
- Role-Based Access Control (RBAC) MUST enforce least-privilege access with three default roles: Admin, User, Guest.
- Admin endpoints MUST require the `[Authorize(Roles = "Admin")]` attribute.

## Performance Standards

All performance targets defined in Principle VII are binding. Additionally:

- Database queries MUST be profiled during development to detect N+1 patterns.
- Caching invalidation strategy MUST be defined for roles/permissions cache.
- Connection pool size MUST be configured based on expected concurrent load.

## Development Workflow

All workflow rules defined in Principle IX are binding. Additionally:

- The Speckit workflow (`speckit.specify` → `speckit.plan` → `speckit.tasks` → `speckit.implement`) SHOULD be used for all new features.
- Constitution compliance MUST be verified at the plan stage (Constitution Check gate) and during code review.

## Governance

This constitution supersedes all other development practices and guidelines in this repository. Amendments require:

1. Documentation of the proposed change with rationale.
2. Review and approval via pull request.
3. A migration plan for any existing code affected by the change.

All pull requests and code reviews MUST verify compliance with these principles. Complexity beyond what is specified here MUST be justified in writing. Use this constitution as the authoritative reference for all development decisions.

**Version**: 1.0.0 | **Ratified**: 2026-02-11 | **Last Amended**: 2026-02-11
