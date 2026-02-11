# Data Model: .NET Core Traditional Authentication

**Branch**: `001-dotnet-traditional-auth` | **Date**: 2026-02-11

## Entity Relationship Diagram

```
┌─────────────────┐       ┌─────────────────┐       ┌─────────────────┐
│      User       │       │    UserRole      │       │      Role       │
├─────────────────┤       ├─────────────────┤       ├─────────────────┤
│ Id (Guid, PK)   │──┐    │ UserId (FK)     │    ┌──│ Id (Guid, PK)   │
│ Email (unique)  │  └───>│ RoleId (FK)     │<───┘  │ Name (unique)   │
│ PasswordHash    │       │ AssignedAt      │       │ Description     │
│ FirstName?      │       └─────────────────┘       │ CreatedAt       │
│ LastName?       │                                  └────────┬────────┘
│ IsActive        │                                           │
│ IsVerified      │       ┌─────────────────┐       ┌────────┴────────┐
│ LastLoginAt?    │       │ RolePermission   │       │                 │
│ CreatedAt       │       ├─────────────────┤       │                 │
│ UpdatedAt       │       │ RoleId (FK)     │<──────┘                 │
└──┬──────────┬───┘       │ PermissionId(FK)│───────┐                 │
   │          │           │ AssignedAt      │       │                 │
   │          │           └─────────────────┘       │                 │
   │          │                                     ▼                 │
   │          │                            ┌─────────────────┐        │
   │          │                            │   Permission    │        │
   │          │                            ├─────────────────┤        │
   │          │                            │ Id (Guid, PK)   │        │
   │          │                            │ Name (unique)   │        │
   │          │                            │ Resource        │        │
   │          │                            │ Action          │        │
   │          │                            │ Description     │        │
   │          │                            │ CreatedAt       │        │
   │          │                            └─────────────────┘        │
   │          │                                                       │
   ▼          ▼                                                       │
┌─────────────────┐       ┌─────────────────┐                        │
│  RefreshToken   │       │    AuditLog     │                        │
├─────────────────┤       ├─────────────────┤                        │
│ Id (Guid, PK)   │       │ Id (Guid, PK)   │                        │
│ UserId (FK)     │       │ UserId? (FK)    │                        │
│ Token (unique)  │       │ Action          │                        │
│ ExpiresAt       │       │ Resource?       │                        │
│ IsRevoked       │       │ IpAddress       │                        │
│ CreatedAt       │       │ UserAgent       │                        │
└─────────────────┘       │ Success         │                        │
                          │ ErrorMessage?   │                        │
                          │ CreatedAt       │                        │
                          └─────────────────┘                        │
```

## Entities

### User

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | Guid | PK, auto-generated | Unique user identifier |
| Email | string | Unique, indexed, not null | User email address |
| PasswordHash | string | Not null | BCrypt-hashed password |
| FirstName | string? | Nullable | User first name |
| LastName | string? | Nullable | User last name |
| IsActive | bool | Default: true | Account active status |
| IsVerified | bool | Default: false | Email verification status |
| LastLoginAt | DateTime? | Nullable | Last successful login timestamp |
| CreatedAt | DateTime | Not null | Account creation timestamp |
| UpdatedAt | DateTime | Not null | Last update timestamp |

**Relationships**: One-to-many with RefreshToken, many-to-many with Role (via UserRole), one-to-many with AuditLog.

### Role

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | Guid | PK, auto-generated | Unique role identifier |
| Name | string | Unique, not null | Role name (Admin, User, Guest) |
| Description | string | Not null | Role description |
| CreatedAt | DateTime | Not null | Role creation timestamp |

**Relationships**: Many-to-many with User (via UserRole), many-to-many with Permission (via RolePermission).

**Seed data**: Admin, User, Guest roles created on first migration.

### Permission

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | Guid | PK, auto-generated | Unique permission identifier |
| Name | string | Unique, not null | Permission name (format: resource:action) |
| Resource | string | Not null | Target resource (e.g., "users", "roles") |
| Action | string | Not null | Allowed action (e.g., "read", "write", "delete") |
| Description | string | Not null | Permission description |
| CreatedAt | DateTime | Not null | Permission creation timestamp |

**Relationships**: Many-to-many with Role (via RolePermission).

### UserRole (Join Table)

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| UserId | Guid | FK → User.Id, composite PK | User reference |
| RoleId | Guid | FK → Role.Id, composite PK | Role reference |
| AssignedAt | DateTime | Not null | Assignment timestamp |

### RolePermission (Join Table)

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| RoleId | Guid | FK → Role.Id, composite PK | Role reference |
| PermissionId | Guid | FK → Permission.Id, composite PK | Permission reference |
| AssignedAt | DateTime | Not null | Assignment timestamp |

### RefreshToken

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | Guid | PK, auto-generated | Unique token identifier |
| UserId | Guid | FK → User.Id, indexed, not null | Owner user |
| Token | string | Unique, indexed, not null | Cryptographic token string |
| ExpiresAt | DateTime | Not null | Token expiration timestamp |
| IsRevoked | bool | Default: false | Revocation status |
| CreatedAt | DateTime | Not null | Token creation timestamp |

**Relationships**: Many-to-one with User.

### AuditLog

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | Guid | PK, auto-generated | Unique log entry identifier |
| UserId | Guid? | FK → User.Id, nullable, indexed | Associated user (null for failed auth) |
| Action | string | Not null | Event type: Login, Logout, PasswordChange, PasswordReset, Registration, FailedLogin |
| Resource | string? | Nullable | Target resource |
| IpAddress | string | Not null | Client IP address |
| UserAgent | string | Not null | Client user agent string |
| Success | bool | Not null | Whether the action succeeded |
| ErrorMessage | string? | Nullable | Error details if action failed |
| CreatedAt | DateTime | Not null | Event timestamp |

**Relationships**: Many-to-one with User (optional).

## Indexes

- `IX_Users_Email` (unique) on User.Email
- `IX_RefreshTokens_Token` (unique) on RefreshToken.Token
- `IX_RefreshTokens_UserId` on RefreshToken.UserId
- `IX_AuditLogs_UserId` on AuditLog.UserId
- `IX_AuditLogs_CreatedAt` on AuditLog.CreatedAt (for time-range queries)
- `IX_Permissions_Name` (unique) on Permission.Name
