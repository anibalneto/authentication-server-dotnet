# Feature Specification: .NET Core Traditional Authentication

**Feature Branch**: `001-dotnet-traditional-auth`
**Created**: 2026-02-11
**Status**: Draft
**Input**: User description: "Build a production-ready ASP.NET Core authentication service with JWT, refresh tokens, RBAC, and full REST API"

## User Scenarios & Testing

### User Story 1 - User Registration & Login (Priority: P1)

A new user visits the application, creates an account with their email and password, and receives access to the system. An existing user provides their credentials and receives tokens to access protected resources.

**Why this priority**: Registration and login are the fundamental authentication flows — without them, no other features can function. This is the core MVP.

**Independent Test**: Can be fully tested by registering a new user, logging in with their credentials, and verifying a valid JWT access token and refresh token are returned. Delivers immediate value as a standalone authentication backend.

**Acceptance Scenarios**:

1. **Given** a new user with a valid email and password meeting complexity requirements, **When** they submit a registration request, **Then** the system creates their account, hashes the password, assigns the default "User" role, and returns an access token and refresh token.
2. **Given** a registered user with valid credentials, **When** they submit a login request, **Then** the system validates their password, issues a JWT access token (15-minute expiry) and refresh token (7-day expiry), and logs the successful login.
3. **Given** a user providing an already-registered email, **When** they attempt to register, **Then** the system rejects the request with a clear error indicating the email is already in use.
4. **Given** a user providing a password that does not meet complexity requirements (8+ chars, uppercase, lowercase, number, special char), **When** they attempt to register, **Then** the system returns validation errors describing which requirements are unmet.
5. **Given** a user providing incorrect credentials, **When** they attempt to login, **Then** the system rejects the request, logs the failed attempt, and does not reveal whether the email or password was wrong.
6. **Given** a user who has failed 5 login attempts within 15 minutes from the same IP, **When** they attempt another login, **Then** the system rate-limits the request and returns an appropriate error.

---

### User Story 2 - Token Management & Session Lifecycle (Priority: P2)

An authenticated user's access token expires, and they use their refresh token to obtain a new access token without re-entering credentials. A user logs out and their tokens are invalidated.

**Why this priority**: Token refresh and logout are essential for maintaining secure, long-lived sessions and allowing users to end sessions explicitly. Required for any production deployment.

**Independent Test**: Can be tested by logging in to get tokens, waiting for or simulating access token expiry, using the refresh token to get new tokens, and then logging out to verify token invalidation.

**Acceptance Scenarios**:

1. **Given** an authenticated user with a valid refresh token, **When** they submit a token refresh request, **Then** the system issues a new access token and a new refresh token, and the old refresh token is invalidated (rotation).
2. **Given** a user with an expired refresh token, **When** they attempt to refresh, **Then** the system rejects the request and requires re-authentication.
3. **Given** a user with a revoked refresh token, **When** they attempt to refresh, **Then** the system rejects the request.
4. **Given** an authenticated user, **When** they submit a logout request, **Then** the system revokes their refresh token and logs the logout event.
5. **Given** a user with a valid access token, **When** they submit a token verification request, **Then** the system confirms the token is valid and returns user details.

---

### User Story 3 - User Profile Management (Priority: P3)

An authenticated user views and updates their own profile information, including changing their password.

**Why this priority**: Profile management allows users to maintain their account details. It depends on authentication (P1) and token management (P2) being in place.

**Independent Test**: Can be tested by logging in, retrieving the user's own profile, updating profile fields, and changing the password followed by re-login with the new password.

**Acceptance Scenarios**:

1. **Given** an authenticated user, **When** they request their profile, **Then** the system returns their profile details (email, first name, last name, roles) without exposing sensitive data like password hashes.
2. **Given** an authenticated user with valid profile updates, **When** they submit a profile update, **Then** the system updates their first name and/or last name and returns the updated profile.
3. **Given** an authenticated user providing their current password and a new valid password, **When** they submit a password change request, **Then** the system updates the password hash, logs the password change event, and confirms success.
4. **Given** an authenticated user providing an incorrect current password, **When** they attempt to change their password, **Then** the system rejects the request.

---

### User Story 4 - Password Reset Flow (Priority: P4)

A user who has forgotten their password requests a password reset, receives a reset token, and uses it to set a new password.

**Why this priority**: Password reset is critical for production but is a secondary flow that depends on the core authentication being functional.

**Independent Test**: Can be tested by requesting a password reset for a known email, using the generated reset token to set a new password, and then logging in with the new password.

**Acceptance Scenarios**:

1. **Given** a registered user who has forgotten their password, **When** they submit a password reset request with their email, **Then** the system generates a reset token (valid for 1 hour) and returns a success message regardless of whether the email exists (to prevent email enumeration).
2. **Given** a valid password reset token, **When** the user submits a new password that meets complexity requirements, **Then** the system updates the password, invalidates the reset token (one-time use), and logs the event.
3. **Given** an expired or already-used reset token, **When** the user attempts to reset their password, **Then** the system rejects the request with an appropriate error.

---

### User Story 5 - Admin User & Role Management (Priority: P5)

An administrator manages users (list, view details) and assigns roles to users to control access levels across the system.

**Why this priority**: Admin functionality is essential for production operations but depends on all other authentication features being in place.

**Independent Test**: Can be tested by logging in as an Admin user, listing all users, viewing a specific user's details, and assigning/removing roles from users. Verified by checking that role changes affect authorization.

**Acceptance Scenarios**:

1. **Given** an authenticated user with the Admin role, **When** they request a list of all users, **Then** the system returns a paginated list of users with their basic details and assigned roles.
2. **Given** an authenticated Admin, **When** they request details for a specific user, **Then** the system returns full profile information including roles, status, and audit metadata.
3. **Given** an authenticated Admin, **When** they assign a role to a user, **Then** the system creates the role assignment and the user gains the permissions of that role.
4. **Given** an authenticated user without the Admin role, **When** they attempt to access any admin endpoint, **Then** the system returns a 403 Forbidden response.

---

### Edge Cases

- What happens when a user attempts to register with a malformed email address? The system MUST reject with a validation error.
- What happens when the database is unavailable during a login attempt? The system MUST return a 503 Service Unavailable with a generic error.
- What happens when a JWT token has been tampered with? The system MUST reject it as invalid.
- What happens when concurrent refresh token requests are made with the same token? Only the first MUST succeed; subsequent requests MUST be rejected (token rotation).
- What happens when an Admin tries to remove the last Admin role from themselves? The system MUST prevent this to avoid lockout.

## Requirements

### Functional Requirements

- **FR-001**: System MUST allow users to register with email and password, validating email format and password complexity (8+ characters, uppercase, lowercase, number, special character).
- **FR-002**: System MUST hash all passwords with BCrypt using a work factor of 12 or higher before storage.
- **FR-003**: System MUST authenticate users via email and password, issuing a JWT access token (15-minute expiry) and refresh token (7-day expiry) on success.
- **FR-004**: System MUST enforce rate limiting on login attempts (5 failed attempts per 15-minute window per IP).
- **FR-005**: System MUST support token refresh with automatic rotation — issuing new tokens and invalidating old refresh tokens.
- **FR-006**: System MUST support user logout by revoking refresh tokens.
- **FR-007**: System MUST provide token verification to confirm token validity and return user details.
- **FR-008**: System MUST allow authenticated users to view and update their own profile (first name, last name).
- **FR-009**: System MUST allow authenticated users to change their password after verifying their current password.
- **FR-010**: System MUST support password reset flow with time-limited tokens (1-hour expiry, single use).
- **FR-011**: System MUST implement Role-Based Access Control with three default roles: Admin, User, Guest.
- **FR-012**: System MUST restrict admin endpoints to users with the Admin role.
- **FR-013**: System MUST provide admin endpoints to list users (paginated), view user details, and assign roles.
- **FR-014**: System MUST log all security-relevant events (login, logout, password change, failed attempts) with IP address, user agent, timestamp, and success status.
- **FR-015**: System MUST provide health check endpoints at `/health` and `/ready`.
- **FR-016**: System MUST return consistent response formats: `{ success, data, message, timestamp }` for success and `{ success, error: { code, message, details }, timestamp }` for errors.
- **FR-017**: System MUST provide OpenAPI documentation accessible via Swagger UI.

### Key Entities

- **User**: Represents a registered user account. Key attributes: unique email, hashed password, first/last name, active status, verification status, login timestamp, creation/update timestamps. Related to roles (many-to-many) and refresh tokens (one-to-many).
- **Role**: Represents an authorization level. Key attributes: unique name, description. Related to users (many-to-many) and permissions (many-to-many). Default roles: Admin, User, Guest.
- **Permission**: Represents a granular access right. Key attributes: unique name in `resource:action` format, resource, action, description. Related to roles (many-to-many).
- **RefreshToken**: Represents a long-lived session token. Key attributes: unique token string, expiration date, revocation status. Belongs to one user.
- **AuditLog**: Records security-relevant events. Key attributes: action type, resource, IP address, user agent, success/failure, error message, timestamp. Optionally linked to a user.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Users can complete account registration in under 30 seconds.
- **SC-002**: Users can log in and receive tokens in under 2 seconds.
- **SC-003**: Token refresh completes in under 1 second.
- **SC-004**: System handles 1,000 concurrent authenticated users without degradation.
- **SC-005**: 95% of API responses return within 200 milliseconds.
- **SC-006**: Zero security vulnerabilities against OWASP Top 10 attack vectors in testing.
- **SC-007**: 80% or higher code coverage across all production code.
- **SC-008**: All API endpoints are documented and testable via Swagger UI.
- **SC-009**: System can be set up and running from source in under 5 minutes using documented instructions.
- **SC-010**: Rate limiting successfully blocks brute-force login attempts after 5 failures within 15 minutes.
