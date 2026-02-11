# Tasks: .NET Core Traditional Authentication

**Input**: Design documents from `/specs/001-dotnet-traditional-auth/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are included as required by the constitution (Principle III: Testing Standards).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization, solution structure, and NuGet dependencies

- [ ] T001 Create .NET solution `AuthServer.sln` with four projects: Auth.Api, Auth.Application, Auth.Domain, Auth.Infrastructure
- [ ] T002 Configure project references: Api → Application → Domain, Infrastructure → Domain, Api → Infrastructure
- [ ] T003 [P] Install NuGet packages per plan.md (JwtBearer, EF Core, Npgsql, BCrypt.Net-Next, FluentValidation, Serilog, Swashbuckle, AutoMapper, xUnit, Moq)
- [ ] T004 [P] Create `.editorconfig` with C# formatting rules and nullable reference types enabled
- [ ] T005 [P] Configure `appsettings.json` and `appsettings.Development.json` with JwtSettings, ConnectionStrings, BcryptSettings sections
- [ ] T006 Configure `Program.cs` with service registration: Serilog, EF Core, JWT Bearer auth, FluentValidation, AutoMapper, Swagger, health checks, CORS, HTTPS redirection

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain entities, database context, repositories, and security infrastructure that ALL user stories depend on

**CRITICAL**: No user story work can begin until this phase is complete

- [ ] T007 Create `User` entity in `Auth.Domain/Entities/User.cs` per data-model.md
- [ ] T008 [P] Create `Role` entity in `Auth.Domain/Entities/Role.cs` per data-model.md
- [ ] T009 [P] Create `Permission` entity in `Auth.Domain/Entities/Permission.cs` per data-model.md
- [ ] T010 [P] Create `UserRole` join entity in `Auth.Domain/Entities/UserRole.cs` per data-model.md
- [ ] T011 [P] Create `RolePermission` join entity in `Auth.Domain/Entities/RolePermission.cs` per data-model.md
- [ ] T012 [P] Create `RefreshToken` entity in `Auth.Domain/Entities/RefreshToken.cs` per data-model.md
- [ ] T013 [P] Create `AuditLog` entity in `Auth.Domain/Entities/AuditLog.cs` per data-model.md
- [ ] T014 Create repository interfaces in `Auth.Domain/Interfaces/`: IUserRepository, IRoleRepository, IRefreshTokenRepository, IAuditLogRepository
- [ ] T015 [P] Create service interfaces in `Auth.Domain/Interfaces/`: IAuthService, IUserService, ITokenService, IPasswordHasher, IJwtTokenGenerator
- [ ] T016 Configure `AppDbContext` in `Auth.Infrastructure/Data/AppDbContext.cs` with entity configurations, relationships, and indexes per data-model.md
- [ ] T017 [P] Create EF Core entity configurations in `Auth.Infrastructure/Data/Configurations/`: UserConfiguration, RoleConfiguration, PermissionConfiguration, RefreshTokenConfiguration, AuditLogConfiguration
- [ ] T018 Create EF Core migration for initial schema
- [ ] T019 Add seed data for default roles (Admin, User, Guest) and admin user in `AppDbContext`
- [ ] T020 Implement `PasswordHasher` in `Auth.Infrastructure/Security/PasswordHasher.cs` (BCrypt, work factor 12)
- [ ] T021 [P] Implement `JwtTokenGenerator` in `Auth.Infrastructure/Security/JwtTokenGenerator.cs` (HS256, 15-min expiry, user claims)
- [ ] T022 Implement repository classes in `Auth.Infrastructure/Repositories/`: UserRepository, RoleRepository, RefreshTokenRepository, AuditLogRepository
- [ ] T023 [P] Create common DTOs in `Auth.Application/DTOs/Common/`: ApiResponse.cs, ApiErrorResponse.cs
- [ ] T024 [P] Create AutoMapper `MappingProfile` in `Auth.Application/Mappings/MappingProfile.cs`
- [ ] T025 Implement `ErrorHandlingMiddleware` in `Auth.Api/Middleware/ErrorHandlingMiddleware.cs` for global exception handling with ProblemDetails
- [ ] T026 [P] Implement `RateLimitingMiddleware` in `Auth.Api/Middleware/RateLimitingMiddleware.cs` (5 failed attempts per 15-min window per IP)

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - User Registration & Login (Priority: P1) MVP

**Goal**: Users can register accounts and login to receive JWT access tokens and refresh tokens.

**Independent Test**: Register a user, login, verify tokens are returned.

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T027 [P] [US1] Unit tests for `AuthService` (register, login) in `tests/Auth.Application.Tests/Services/AuthServiceTests.cs`
- [ ] T028 [P] [US1] Unit tests for `RegisterRequestValidator` in `tests/Auth.Application.Tests/Validators/ValidatorTests.cs`
- [ ] T029 [P] [US1] Integration tests for POST /api/auth/register and POST /api/auth/login in `tests/Auth.Api.Tests/Integration/AuthIntegrationTests.cs`

### Implementation for User Story 1

- [ ] T030 [P] [US1] Create auth DTOs in `Auth.Application/DTOs/Auth/`: RegisterRequest.cs, LoginRequest.cs, AuthResponse.cs
- [ ] T031 [P] [US1] Create `RegisterRequestValidator` in `Auth.Application/Validators/RegisterRequestValidator.cs` (email format, password complexity)
- [ ] T032 [P] [US1] Create `LoginRequestValidator` in `Auth.Application/Validators/LoginRequestValidator.cs`
- [ ] T033 [US1] Implement `AuthService` in `Auth.Application/Services/AuthService.cs` (Register, Login methods)
- [ ] T034 [US1] Implement `TokenService` in `Auth.Application/Services/TokenService.cs` (GenerateTokenPair method)
- [ ] T035 [US1] Create `AuthController` in `Auth.Api/Controllers/AuthController.cs` with POST /api/auth/register and POST /api/auth/login endpoints

**Checkpoint**: User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Token Management & Session Lifecycle (Priority: P2)

**Goal**: Users can refresh tokens, verify tokens, and logout with token revocation.

**Independent Test**: Login, refresh token (verify rotation), verify token, logout (verify revocation).

### Tests for User Story 2

- [ ] T036 [P] [US2] Unit tests for `TokenService` (refresh, revoke, verify) in `tests/Auth.Application.Tests/Services/TokenServiceTests.cs`
- [ ] T037 [P] [US2] Integration tests for POST /api/auth/refresh, POST /api/auth/verify, POST /api/auth/logout in `tests/Auth.Api.Tests/Integration/AuthIntegrationTests.cs`

### Implementation for User Story 2

- [ ] T038 [P] [US2] Create `RefreshTokenRequest` DTO in `Auth.Application/DTOs/Auth/RefreshTokenRequest.cs`
- [ ] T039 [US2] Add RefreshToken, Verify, and RevokeToken methods to `TokenService`
- [ ] T040 [US2] Add POST /api/auth/refresh, POST /api/auth/verify, POST /api/auth/logout endpoints to `AuthController`

**Checkpoint**: User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - User Profile Management (Priority: P3)

**Goal**: Authenticated users can view/update their profile and change their password.

**Independent Test**: Login, get profile, update profile fields, change password, re-login with new password.

### Tests for User Story 3

- [ ] T041 [P] [US3] Unit tests for `UserService` in `tests/Auth.Application.Tests/Services/UserServiceTests.cs`
- [ ] T042 [P] [US3] Unit tests for `ChangePasswordRequestValidator` in `tests/Auth.Application.Tests/Validators/ValidatorTests.cs`
- [ ] T043 [P] [US3] Integration tests for GET/PUT /api/users/me, POST /api/users/change-password in `tests/Auth.Api.Tests/Integration/AuthIntegrationTests.cs`

### Implementation for User Story 3

- [ ] T044 [P] [US3] Create user DTOs in `Auth.Application/DTOs/User/`: UserProfileResponse.cs, UpdateProfileRequest.cs, ChangePasswordRequest.cs
- [ ] T045 [P] [US3] Create `ChangePasswordRequestValidator` in `Auth.Application/Validators/ChangePasswordRequestValidator.cs`
- [ ] T046 [US3] Implement `UserService` in `Auth.Application/Services/UserService.cs` (GetProfile, UpdateProfile, ChangePassword)
- [ ] T047 [US3] Create `UserController` in `Auth.Api/Controllers/UserController.cs` with GET/PUT /api/users/me and POST /api/users/change-password

**Checkpoint**: User Stories 1, 2, AND 3 should all work independently

---

## Phase 6: User Story 4 - Password Reset Flow (Priority: P4)

**Goal**: Users can request password reset and use a token to set a new password.

**Independent Test**: Request reset, use token to set new password, login with new password.

### Tests for User Story 4

- [ ] T048 [P] [US4] Unit tests for password reset in `AuthService` in `tests/Auth.Application.Tests/Services/AuthServiceTests.cs`
- [ ] T049 [P] [US4] Integration tests for POST /api/auth/password-reset and POST /api/auth/password-reset/{token} in `tests/Auth.Api.Tests/Integration/AuthIntegrationTests.cs`

### Implementation for User Story 4

- [ ] T050 [P] [US4] Create password reset DTOs in `Auth.Application/DTOs/Auth/`: PasswordResetRequest.cs, PasswordResetConfirmRequest.cs
- [ ] T051 [P] [US4] Create `PasswordResetConfirmValidator` in `Auth.Application/Validators/PasswordResetConfirmValidator.cs`
- [ ] T052 [US4] Add RequestPasswordReset and ResetPassword methods to `AuthService`
- [ ] T053 [US4] Add POST /api/auth/password-reset and POST /api/auth/password-reset/{token} endpoints to `AuthController`

**Checkpoint**: User Stories 1-4 should all work independently

---

## Phase 7: User Story 5 - Admin User & Role Management (Priority: P5)

**Goal**: Admin users can list users, view user details, and assign roles.

**Independent Test**: Login as Admin, list users, view user detail, assign role to user, verify authorization.

### Tests for User Story 5

- [ ] T054 [P] [US5] Unit tests for admin operations in `UserService` in `tests/Auth.Application.Tests/Services/UserServiceTests.cs`
- [ ] T055 [P] [US5] Integration tests for admin endpoints in `tests/Auth.Api.Tests/Integration/AdminIntegrationTests.cs`
- [ ] T056 [P] [US5] Security tests: verify non-Admin users get 403 on admin endpoints in `tests/Auth.Api.Tests/Integration/AdminIntegrationTests.cs`

### Implementation for User Story 5

- [ ] T057 [P] [US5] Create admin DTOs in `Auth.Application/DTOs/Admin/`: UserListResponse.cs, UserDetailResponse.cs, AssignRoleRequest.cs
- [ ] T058 [US5] Add admin methods to `UserService`: ListUsers (paginated), GetUserById, AssignRole
- [ ] T059 [US5] Create `AdminController` in `Auth.Api/Controllers/AdminController.cs` with [Authorize(Roles = "Admin")] and endpoints: GET /api/admin/users, GET /api/admin/users/{id}, POST /api/admin/users/{id}/roles

**Checkpoint**: All user stories should now be independently functional

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, Docker, security hardening, and final quality checks

- [ ] T060 [P] Configure Swagger/Swashbuckle with XML documentation comments and JWT bearer auth in Swagger UI
- [ ] T061 [P] Create `Dockerfile` for Auth.Api
- [ ] T062 [P] Create `docker-compose.yml` with auth-api and postgres services
- [ ] T063 [P] Add XML documentation comments to all public API controller methods
- [ ] T064 Security hardening: verify HTTPS redirect, security headers, CORS configuration
- [ ] T065 Run full test suite and verify 80%+ code coverage
- [ ] T066 Run quickstart.md validation: follow the quickstart guide end-to-end

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - User stories can proceed in parallel (if staffed) or sequentially in priority order (P1 → P2 → P3 → P4 → P5)
- **Polish (Phase 8)**: Depends on all user stories being complete

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- DTOs and validators before services
- Services before controllers
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel
- All tests for a user story marked [P] can run in parallel
- DTOs and validators within a story marked [P] can run in parallel

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Add User Story 4 → Test independently → Deploy/Demo
6. Add User Story 5 → Test independently → Deploy/Demo
7. Each story adds value without breaking previous stories

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
