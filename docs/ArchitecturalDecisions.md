# AtomicLMS Core Architectural Decisions

## Overview
Key architectural and implementation decisions for AtomicLMS Core, a headless multi-tenant SAAS LMS.

## Multi-Tenancy Strategy

### Database Architecture
- Database-per-tenant architecture for complete data isolation
- Shared database for Solutions feature bucket (tenants, superadmin)
- Each tenant has their own database instance
- Tenant database naming convention: AtomicLms_Tenant_{TenantId}
- Connection strings resolved dynamically via IConnectionStringProvider

### Tenant Database Security
- Each tenant database contains a `__tenant_identity` table with cryptographic validation
- ITenantDatabaseValidator performs cached validation before any database connection
- Tenant identity records automatically created during database migration
- Validation uses SHA256 hash of TenantId + DatabaseName + CreatedAt + Secret
- Validation results cached for 1 hour (success) or 5 minutes (failure) for performance
- All validation attempts logged for security audit

### Feature Bucket Organization
- **Solution**: Management of tenants and super-admin level operations
- **Administration**: Tenant manages their setup
- **Learning**: Setting up courses
- **Learners**: Setting up users
- **Engagement**: Actions by learners

### API Path Structure
- `api/v{version}/solution/...` - Solution feature bucket
- `api/v{version}/administration/...` - Administration feature bucket
- `api/v{version}/learning/...` - Learning feature bucket
- `api/v{version}/learners/...` - Learners feature bucket
- `api/v{version}/engagement/...` - Engagement feature bucket

### Intelligent Tenant Resolution
- TenantResolutionMiddleware handles both validation and authorization with intelligent logic:
  - **Single tenant claim + No header**: Uses tenant from claim automatically
  - **Single tenant claim + Header matches**: Uses tenant ID (validates consistency)
  - **Multiple tenant claims + Header matches one**: Uses specified tenant from header
  - **Multiple tenant claims + No header**: Returns 400 with available tenant list
  - **No tenant claims**: Only valid for Solution feature bucket endpoints
  - **Header doesn't match claims**: Returns 403 Forbidden
- SuperAdmin role bypasses tenant resolution for Solution endpoints
- SuperAdmin accessing tenant-specific endpoints must specify X-Tenant-Id header
- Eliminates need for X-Tenant-Id header when user has single tenant access
- Prevents privilege escalation between tenant boundaries

### Database Context Strategy
- Solutions Feature Bucket uses SolutionsDbContext (cross-tenant data)
- All other feature buckets use TenantDbContext (tenant-specific databases)
- No tenant filtering needed in repositories (database-level isolation)
- Automatic database provisioning on tenant creation via ITenantDatabaseService

## Identity Provider Abstraction
- Generic external identity provider abstraction
- User entity uses `ExternalUserId` instead of provider-specific naming
- Repository methods use `GetByExternalUserIdAsync`, `ExternalUserIdExistsAsync`
- Allows switching between Auth0, Azure AD, custom providers without code changes

## Domain-Specific Decisions

### LMS-Specific Conventions
- Event sourcing for critical student progress tracking
- Enrollment state machines should be explicit
- Grade calculations must be idempotent
- Progress tracking should be eventually consistent
- Soft deletes by default for all student/course data
- Immutable audit logs for all grade/enrollment changes

### Entity Design
- All major entities should have metadata (IDictionary<string,string>)
- Metadata should not be returned in listing actions
- Audit Fields should be set in the Infrastructure project, and readonly in the models

## Development and Deployment

### Database Provisioning
- Tenant databases created/migrated independently
- Automatic database provisioning on tenant creation
- Code-first migrations for schema management
- Tenant identity validation integrated into migration process

### Configuration Requirements
- TenantValidation:Secret configuration for cryptographic validation
- Connection string templates for dynamic tenant database connections
- Master database connection for tenant database management operations

## Authentication & Authorization

### JWT Bearer Authentication
- Auth0 integration via Microsoft.AspNetCore.Authentication.JwtBearer
- JWT configuration follows IOptions pattern with JwtOptions class
- Configurable token validation parameters (audience, issuer, lifetime, HTTPS metadata)
- 5-minute clock skew tolerance for token expiration

### Tenant ID Claims Processing
- Supports flexible tenant_id claim mapping for Auth0 custom namespaces
- OnTokenValidated event maps custom namespace claims (e.g., `https://custom/tenant_id`) to standard `tenant_id` claim
- Preserves existing `tenant_id` claims while adding mapped claims
- Compatible with Auth0 Rules/Actions that add custom tenant claims

### Authorization Model
- Controllers protected with `[Authorize]` attribute for authenticated users
- Solution feature bucket requires `[Authorize(Roles = "superadmin")]` for administrative operations
- TenantResolutionMiddleware validates user's tenant access post-authentication
- SuperAdmin role can access any tenant via X-Tenant-Id header

### Security Integration
- JWT authentication middleware positioned before tenant resolution middleware
- Authentication validates token signatures and claims
- Authorization enforces role-based and tenant-based access control
- Swagger UI includes JWT Bearer token input for API testing