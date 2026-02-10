# Spec Kit Workflow - .NET Core Traditional Authentication

**Complete Spec Kit workflow for .NET Core Traditional Authentication boilerplate**

Use these prompts in order with Spec Kit's slash commands in your AI coding assistant.

---

## 1. /speckit.constitution - Project Principles

```
/speckit.constitution

Create comprehensive project principles for .NET Core Traditional Authentication boilerplate.

PROJECT CONTEXT:
- Technology: ASP.NET Core with C#
- Authentication: Traditional (JWT-based username/password)
- Database: PostgreSQL with Entity Framework Core
- Purpose: Production-ready authentication microservice boilerplate

Principles should cover:

1. CODE QUALITY STANDARDS
   - .NET 8.0 LTS enforcement
   - StyleCop analyzers with zero warnings
   - .editorconfig for consistent formatting
   - 80%+ test coverage minimum
   - No TODO comments without issue references
   - Code review required for all changes
   - Nullable reference types enabled
   - Async/await best practices

2. SECURITY REQUIREMENTS
   - OWASP Top 10 compliance mandatory
   - No hardcoded secrets or credentials
   - All passwords hashed with BCrypt.Net (work factor 12+)
   - SQL injection prevention (EF Core parameterized queries)
   - XSS protection enabled
   - CSRF protection for state-changing operations
   - Rate limiting middleware
   - JWT Bearer authentication
   - Token expiration enforced
   - HTTPS redirection in production
   - Security headers middleware

3. TESTING STANDARDS
   - Minimum 80% code coverage
   - xUnit for unit testing
   - WebApplicationFactory for integration testing
   - Moq for mocking
   - Test database with Testcontainers
   - Unit tests for all services
   - Integration tests for all API endpoints
   - Security test cases

4. API DESIGN
   - RESTful conventions strictly followed
   - Consistent response format (success/error)
   - OpenAPI 3.0 with Swashbuckle
   - Swagger UI documentation at /api/docs
   - API versioning strategy
   - Proper HTTP status codes
   - FluentValidation for request validation

5. ASP.NET CORE SPECIFIC STANDARDS
   - Minimal APIs or Controllers (Controllers recommended)
   - Dependency injection via constructor
   - [ApiController] attribute
   - [Authorize] for protected endpoints
   - Options pattern for configuration
   - ILogger<T> for logging
   - ProblemDetails for error responses

6. DOCUMENTATION
   - README with quick start (5-minute setup)
   - XML documentation comments
   - Architecture documentation
   - API examples with curl commands
   - appsettings.json documentation
   - Deployment guide
   - Docker setup documented
   - Troubleshooting section

7. PERFORMANCE
   - API response time <200ms (p95)
   - Database connection pooling
   - Efficient EF Core queries with AsNoTracking
   - Caching for roles/permissions with IMemoryCache
   - Pagination for list endpoints
   - No N+1 query problems

8. RELIABILITY
   - Graceful error handling with exception filters
   - Serilog structured logging (JSON format)
   - Health check endpoints (/health, /ready)
   - Database migration on startup (optional)
   - Graceful shutdown
   - Transaction management

9. DEVELOPMENT WORKFLOW
   - Feature branches from main
   - Pull request required before merge
   - Automated tests must pass
   - Commit messages follow conventional commits
   - Environment-specific appsettings

10. C# / .NET STANDARDS
    - Nullable reference types enabled
    - Record types for DTOs
    - Pattern matching where appropriate
    - LINQ for queries
    - AutoMapper for DTO mapping
    - Extension methods for clean code
```

---

## 2. /speckit.specify - Create Specification

```
/speckit.specify

Create detailed specification for .NET Core Traditional Authentication boilerplate.

TECHNOLOGY STACK:
- Framework: .NET 8.0 LTS
- Language: C# 12
- API: ASP.NET Core Web API
- Authentication: JWT Bearer (Microsoft.AspNetCore.Authentication.JwtBearer)
- Password Hashing: BCrypt.Net-Next
- Database: PostgreSQL 15+
- ORM: Entity Framework Core 8
- Validation: FluentValidation.AspNetCore
- Testing: xUnit + Moq + WebApplicationFactory
- Logging: Serilog.AspNetCore
- Documentation: Swashbuckle.AspNetCore (Swagger/OpenAPI)
- Mapping: AutoMapper

FEATURE OVERVIEW:
Build a production-ready ASP.NET Core authentication service that:
- Handles user registration with email validation
- Manages user login with credentials
- Issues JWT access tokens (15-minute expiration)
- Provides refresh token mechanism (7-day expiration)
- Implements logout with token revocation
- Supports password reset flow
- Enforces role-based access control with [Authorize(Roles = "")]
- Provides complete REST API with OpenAPI documentation

AUTHENTICATION FEATURES REQUIRED:

1. User Registration
   - Email validation (valid email format)
   - Password complexity validation with FluentValidation (8+ chars, uppercase, lowercase, number, special char)
   - Password hashing with BCrypt.Net (work factor 12)
   - Duplicate email prevention
   - User activation (active by default)
   - Registration audit logging

2. User Login
   - Credential validation (email + password)
   - JWT access token generation (15-minute expiry)
   - Refresh token generation (7-day expiry)
   - Rate limiting middleware (5 failed attempts per 15 minutes)
   - Audit logging of login attempts
   - Return user details with tokens

3. Token Management
   - POST /api/auth/refresh - Get new access token with refresh token
   - Token validation middleware (JWT Bearer)
   - Automatic token rotation on refresh
   - Token expiration enforcement
   - Expired token rejection with proper error

4. User Logout
   - Token revocation (add to blacklist or remove refresh token)
   - Refresh token invalidation in database
   - Session cleanup
   - Logout audit logging

5. Password Reset
   - POST /api/auth/password-reset - Request password reset
   - Generate reset token (valid for 1 hour)
   - Email notification with reset link
   - POST /api/auth/password-reset/{token} - Reset password
   - Token validation and one-time use
   - Password change audit logging

AUTHORIZATION FEATURES REQUIRED:

1. ASP.NET Core Authorization
   - [Authorize] attribute for protected endpoints
   - [Authorize(Roles = "Admin")] for admin-only endpoints
   - Policy-based authorization
   - Custom authorization handlers (optional)

2. Role-Based Access Control (RBAC)
   - Three default roles: Admin, User, Guest
   - Role assignment to users (many-to-many)
   - Dynamic permission assignment to roles
   - Custom authorization policies

3. User Profile Management
   - GET /api/users/me - Get own profile
   - PUT /api/users/me - Update own profile
   - POST /api/users/change-password - Change password
   - Admin endpoints:
     * GET /api/admin/users - List all users
     * GET /api/admin/users/{id} - Get user details
     * POST /api/admin/users/{id}/roles - Assign role to user

DATA MODEL REQUIREMENTS (EF Core Entities):

1. User Entity
   - Id (Guid, primary key)
   - Email (string, unique, indexed)
   - PasswordHash (string)
   - FirstName (string, nullable)
   - LastName (string, nullable)
   - IsActive (bool, default true)
   - IsVerified (bool, default false)
   - LastLoginAt (DateTime?, nullable)
   - CreatedAt (DateTime)
   - UpdatedAt (DateTime)
   - Roles (ICollection<UserRole> - many-to-many)

2. Role Entity
   - Id (Guid)
   - Name (string, unique)
   - Description (string)
   - CreatedAt (DateTime)
   - Permissions (ICollection<RolePermission>)
   - Users (ICollection<UserRole>)

3. Permission Entity
   - Id (Guid)
   - Name (string, unique, format: resource:action)
   - Resource (string)
   - Action (string)
   - Description (string)
   - CreatedAt (DateTime)
   - Roles (ICollection<RolePermission>)

4. RefreshToken Entity
   - Id (Guid)
   - UserId (Guid, foreign key)
   - Token (string, unique, indexed)
   - ExpiresAt (DateTime)
   - IsRevoked (bool, default false)
   - CreatedAt (DateTime)
   - User (User navigation property)

5. AuditLog Entity
   - Id (Guid)
   - UserId (Guid?, nullable, foreign key)
   - Action (string: Login, Logout, PasswordChange, etc.)
   - Resource (string, nullable)
   - IpAddress (string)
   - UserAgent (string)
   - Success (bool)
   - ErrorMessage (string, nullable)
   - CreatedAt (DateTime)
   - User (User navigation property)

API ENDPOINTS SPECIFICATION:

Authentication:
- POST /api/auth/register
  * Request: { email, password, firstName?, lastName? }
  * Response: { success, data: { user, accessToken, refreshToken }, message, timestamp }
  
- POST /api/auth/login
  * Request: { email, password }
  * Response: { success, data: { user, accessToken, refreshToken }, message, timestamp }
  
- POST /api/auth/logout
  * Headers: Authorization: Bearer {token}
  * Response: { success, message, timestamp }
  
- POST /api/auth/refresh
  * Request: { refreshToken }
  * Response: { success, data: { accessToken, refreshToken }, message, timestamp }
  
- POST /api/auth/verify
  * Headers: Authorization: Bearer {token}
  * Response: { success, data: { valid: true, user }, message, timestamp }
  
- POST /api/auth/password-reset
  * Request: { email }
  * Response: { success, message, timestamp }

User Management:
- GET /api/users/me
- PUT /api/users/me
- POST /api/users/change-password

Admin/RBAC:
- GET /api/admin/users ([Authorize(Roles = "Admin")])
- GET /api/admin/users/{id}
- POST /api/admin/users/{id}/roles
- GET /api/roles
- POST /api/roles
- GET /api/permissions

Health Checks:
- GET /health
- GET /ready

RESPONSE FORMAT:
Success: { success: true, data, message, timestamp }
Error: { success: false, error: { code, message, details }, timestamp }
```

---

## 3. /speckit.plan - Implementation Plan

```
/speckit.plan

PROJECT STRUCTURE:

dotnet-traditional-auth/
├── .speckit/
├── src/
│   ├── Auth.Api/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── UserController.cs
│   │   │   └── AdminController.cs
│   │   ├── Middleware/
│   │   │   ├── ErrorHandlingMiddleware.cs
│   │   │   └── RateLimitingMiddleware.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── Auth.Application/
│   │   ├── DTOs/
│   │   ├── Services/
│   │   │   ├── AuthService.cs
│   │   │   ├── UserService.cs
│   │   │   └── TokenService.cs
│   │   ├── Validators/
│   │   └── Mappings/
│   ├── Auth.Domain/
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Role.cs
│   │   │   └── RefreshToken.cs
│   │   └── Interfaces/
│   └── Auth.Infrastructure/
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   └── Migrations/
│       ├── Repositories/
│       └── Security/
│           ├── PasswordHasher.cs
│           └── JwtTokenGenerator.cs
├── tests/
│   ├── Auth.Api.Tests/
│   └── Auth.Application.Tests/
├── Dockerfile
├── docker-compose.yml
└── README.md


NUGET PACKAGES:
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.EntityFrameworkCore
- Npgsql.EntityFrameworkCore.PostgreSQL
- BCrypt.Net-Next
- FluentValidation.AspNetCore
- Serilog.AspNetCore
- Swashbuckle.AspNetCore
- AutoMapper.Extensions.Microsoft.DependencyInjection
- xUnit
- Moq

IMPLEMENTATION PHASES:

PHASE 1: PROJECT SETUP (Day 1)
- Create .NET solution with projects
- Configure project references
- Install NuGet packages
- Setup appsettings.json
- Configure Serilog

PHASE 2: DATABASE (Day 2)
- Create EF Core entities
- Configure DbContext
- Create migrations
- Seed data

PHASE 3: AUTHENTICATION (Days 3-4)
- Implement password hashing
- Implement JWT generation
- Create AuthService
- Configure JWT Bearer authentication

PHASE 4: AUTHORIZATION (Day 5)
- Configure authorization policies
- Create admin endpoints

PHASE 5: TESTING (Days 6-7)
- xUnit tests
- Integration tests with WebApplicationFactory

PHASE 6: DOCUMENTATION (Day 8)
- Swagger UI
- README
- Docker setup
```

---

## 4. /speckit.tasks - Task Breakdown

```
/speckit.tasks

PHASE 1: PROJECT SETUP
- [ ] SETUP-001: Create .NET solution
  * dotnet new sln
  * Complexity: Low

- [ ] SETUP-002: Create API project
  * dotnet new webapi -n Auth.Api
  * Complexity: Low

- [ ] SETUP-003: Create class library projects
  * Application, Domain, Infrastructure
  * Complexity: Low

- [ ] SETUP-004: Install NuGet packages
  * JWT Bearer, EF Core, BCrypt, FluentValidation, Serilog
  * Complexity: Low

- [ ] SETUP-005: Configure appsettings.json
  * Database, JWT, logging settings
  * Complexity: Low

PHASE 2: DATABASE
- [ ] DB-001: Create User entity
  * Security Checkpoint: Yes (PasswordHash)
  
- [ ] DB-002: Create Role entity
- [ ] DB-003: Create Permission entity
- [ ] DB-004: Create RefreshToken entity
- [ ] DB-005: Create AuditLog entity
- [ ] DB-006: Configure DbContext
- [ ] DB-007: Create EF Core migrations
- [ ] DB-008: Create seed data

PHASE 3: AUTHENTICATION
- [ ] AUTH-001: Create password hasher (BCrypt)
  * Work factor 12
  * Security Checkpoint: Yes

- [ ] AUTH-002: Create JWT token generator
  * HS256, 15min expiration
  * Security Checkpoint: Yes

- [ ] AUTH-003: Create AuthService
  * Register, Login, Logout
  * Complexity: High

- [ ] AUTH-004: Create TokenService
  * Generate, Refresh, Revoke
  * Complexity: High

- [ ] AUTH-005: Configure JWT Bearer authentication
  * Program.cs configuration
  * Security Checkpoint: Yes

- [ ] AUTH-006: Create AuthController
  * All auth endpoints
  * Complexity: High

- [ ] AUTH-007: Implement rate limiting middleware
  * Security Checkpoint: Yes

PHASE 4: AUTHORIZATION
- [ ] AUTHZ-001: Configure authorization policies
- [ ] AUTHZ-002: Create AdminController
  * [Authorize(Roles = "Admin")]
  * Security Checkpoint: Yes

- [ ] AUTHZ-003: Create UserController

PHASE 5: TESTING
- [ ] TEST-001: Setup xUnit
- [ ] TEST-002: Unit tests for AuthService
- [ ] TEST-003: Integration tests with WebApplicationFactory
- [ ] TEST-004: Security tests
- [ ] TEST-005: Verify 80%+ coverage

PHASE 6: DOCUMENTATION
- [ ] DOC-001: Configure Swashbuckle
- [ ] DOC-002: Create README
- [ ] DOC-003: Create Dockerfile
- [ ] DOC-004: Create docker-compose.yml
```

---

## 5. /speckit.implement - Execute Implementation

```
/speckit.implement

IMPLEMENTATION CHECKLIST:

✅ PROJECT SETUP:
- [ ] .NET 8.0 solution created
- [ ] Projects: Api, Application, Domain, Infrastructure
- [ ] NuGet packages installed
- [ ] appsettings.json configured

✅ DATABASE:
- [ ] EF Core entities
- [ ] DbContext configured
- [ ] Migrations created
- [ ] Seed data (roles)

✅ AUTHENTICATION:
- [ ] BCrypt password hasher (work factor 12)
- [ ] JWT token generator (HS256, 15min)
- [ ] AuthService (Register, Login)
- [ ] TokenService (Generate, Refresh, Revoke)
- [ ] JWT Bearer authentication configured
- [ ] AuthController

✅ AUTHORIZATION:
- [ ] [Authorize] attributes
- [ ] [Authorize(Roles = "Admin")]
- [ ] Authorization policies

✅ TESTING:
- [ ] xUnit tests
- [ ] WebApplicationFactory integration tests
- [ ] 80%+ coverage

✅ DOCUMENTATION:
- [ ] Swagger UI at /api/docs
- [ ] README
- [ ] Dockerfile
- [ ] docker-compose.yml

APPSETTINGS.JSON EXAMPLE:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=authdb;Username=postgres;Password=password"
  },
  "JwtSettings": {
    "Secret": "your-super-secret-jwt-key-min-32-chars",
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

Begin with project setup and database layer, then implement JWT authentication.
```

---

**Document Version:** 1.0  
**Created:** February 9, 2026  
**Stack:** .NET Core Traditional Authentication
