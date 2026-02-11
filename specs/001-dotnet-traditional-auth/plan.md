# Implementation Plan: .NET Core Traditional Authentication

**Branch**: `001-dotnet-traditional-auth` | **Date**: 2026-02-11 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-dotnet-traditional-auth/spec.md`

## Summary

Build a production-ready ASP.NET Core 8.0 authentication microservice providing user registration, JWT-based login, refresh token rotation, password reset, role-based access control (RBAC), and comprehensive audit logging. The service follows clean architecture with four projects (Api, Application, Domain, Infrastructure) backed by PostgreSQL via Entity Framework Core.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0 LTS
**Primary Dependencies**: ASP.NET Core Web API, Microsoft.AspNetCore.Authentication.JwtBearer, BCrypt.Net-Next, FluentValidation.AspNetCore, AutoMapper, Serilog.AspNetCore, Swashbuckle.AspNetCore
**Storage**: PostgreSQL 15+ via Npgsql.EntityFrameworkCore.PostgreSQL
**Testing**: xUnit + Moq + WebApplicationFactory + Testcontainers
**Target Platform**: Linux server (Docker container)
**Project Type**: Web API (single backend service)
**Performance Goals**: <200ms p95 response time, 1000 concurrent users
**Constraints**: OWASP Top 10 compliance, 80%+ test coverage, zero hardcoded secrets
**Scale/Scope**: Authentication microservice for 10k+ registered users

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Code Quality Standards | PASS | .NET 8.0 LTS, StyleCop, .editorconfig, nullable reference types planned |
| II. Security Requirements | PASS | BCrypt (WF 12), JWT HS256, rate limiting, no hardcoded secrets, HTTPS redirect |
| III. Testing Standards | PASS | xUnit, WebApplicationFactory, Moq, Testcontainers, 80% coverage target |
| IV. API Design | PASS | RESTful, consistent response format, FluentValidation, Swashbuckle/OpenAPI |
| V. ASP.NET Core Standards | PASS | Controllers, constructor DI, [ApiController], [Authorize], Options pattern, ILogger<T>, ProblemDetails |
| VI. Documentation | PASS | README quickstart, XML docs, Swagger UI, curl examples, Docker setup |
| VII. Performance | PASS | Connection pooling, AsNoTracking, IMemoryCache, pagination, N+1 prevention |
| VIII. Reliability | PASS | ErrorHandlingMiddleware, Serilog JSON, /health + /ready, transactions |
| IX. Development Workflow | PASS | Feature branches, PR required, conventional commits, env-specific config |
| X. C# / .NET Conventions | PASS | Nullable types, record DTOs, LINQ, AutoMapper, extension methods |

## Project Structure

### Documentation (this feature)

```text
specs/001-dotnet-traditional-auth/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── auth-api.yaml    # OpenAPI 3.0 contract
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
Auth.Api/
├── Controllers/
│   ├── AuthController.cs
│   ├── UserController.cs
│   └── AdminController.cs
├── Middleware/
│   ├── ErrorHandlingMiddleware.cs
│   └── RateLimitingMiddleware.cs
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── Auth.Api.csproj

Auth.Application/
├── DTOs/
│   ├── Auth/
│   │   ├── RegisterRequest.cs
│   │   ├── LoginRequest.cs
│   │   ├── RefreshTokenRequest.cs
│   │   ├── PasswordResetRequest.cs
│   │   ├── PasswordResetConfirmRequest.cs
│   │   └── AuthResponse.cs
│   ├── User/
│   │   ├── UserProfileResponse.cs
│   │   ├── UpdateProfileRequest.cs
│   │   └── ChangePasswordRequest.cs
│   ├── Admin/
│   │   ├── UserListResponse.cs
│   │   ├── UserDetailResponse.cs
│   │   └── AssignRoleRequest.cs
│   └── Common/
│       ├── ApiResponse.cs
│       └── ApiErrorResponse.cs
├── Services/
│   ├── AuthService.cs
│   ├── UserService.cs
│   └── TokenService.cs
├── Validators/
│   ├── RegisterRequestValidator.cs
│   ├── LoginRequestValidator.cs
│   ├── ChangePasswordRequestValidator.cs
│   └── PasswordResetConfirmValidator.cs
├── Mappings/
│   └── MappingProfile.cs
└── Auth.Application.csproj

Auth.Domain/
├── Entities/
│   ├── User.cs
│   ├── Role.cs
│   ├── Permission.cs
│   ├── UserRole.cs
│   ├── RolePermission.cs
│   ├── RefreshToken.cs
│   └── AuditLog.cs
├── Interfaces/
│   ├── IUserRepository.cs
│   ├── IRoleRepository.cs
│   ├── IRefreshTokenRepository.cs
│   ├── IAuditLogRepository.cs
│   ├── IAuthService.cs
│   ├── IUserService.cs
│   ├── ITokenService.cs
│   ├── IPasswordHasher.cs
│   └── IJwtTokenGenerator.cs
└── Auth.Domain.csproj

Auth.Infrastructure/
├── Data/
│   ├── AppDbContext.cs
│   ├── Configurations/
│   │   ├── UserConfiguration.cs
│   │   ├── RoleConfiguration.cs
│   │   ├── PermissionConfiguration.cs
│   │   ├── RefreshTokenConfiguration.cs
│   │   └── AuditLogConfiguration.cs
│   └── Migrations/
├── Repositories/
│   ├── UserRepository.cs
│   ├── RoleRepository.cs
│   ├── RefreshTokenRepository.cs
│   └── AuditLogRepository.cs
├── Security/
│   ├── PasswordHasher.cs
│   └── JwtTokenGenerator.cs
└── Auth.Infrastructure.csproj

tests/
├── Auth.Api.Tests/
│   ├── Controllers/
│   │   ├── AuthControllerTests.cs
│   │   ├── UserControllerTests.cs
│   │   └── AdminControllerTests.cs
│   ├── Integration/
│   │   ├── AuthIntegrationTests.cs
│   │   └── AdminIntegrationTests.cs
│   └── Auth.Api.Tests.csproj
└── Auth.Application.Tests/
    ├── Services/
    │   ├── AuthServiceTests.cs
    │   ├── UserServiceTests.cs
    │   └── TokenServiceTests.cs
    ├── Validators/
    │   └── ValidatorTests.cs
    └── Auth.Application.Tests.csproj

Dockerfile
docker-compose.yml
AuthServer.sln
.editorconfig
README.md
```

**Structure Decision**: Clean architecture with four projects (Api → Application → Domain ← Infrastructure). Domain has zero external dependencies. Infrastructure implements Domain interfaces. Application orchestrates business logic. Api handles HTTP concerns. This matches the project requirements and constitution principles.

## Complexity Tracking

No constitution violations to justify. The four-project structure is standard clean architecture for .NET and aligns directly with Principle V (ASP.NET Core Standards) and the repository's existing documentation.
