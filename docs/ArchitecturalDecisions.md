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
- API calls in all feature buckets except Solution require X-Tenant-Id header

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