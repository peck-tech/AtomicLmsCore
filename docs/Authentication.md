# Authentication Management

## Overview
AtomicLMS Core supports two OAuth 2.0 authentication flows through Auth0:
- **Authorization Code Flow with PKCE**: For user authentication (web apps, mobile apps)
- **Client Credentials Flow**: For machine-to-machine authentication (services, APIs)

## Authentication Flows

### Authorization Code Flow with PKCE
- **Use Case**: Interactive user authentication
- **Token Contains**: User identity (`sub` claim), tenant associations
- **User Context**: Extracted directly from JWT token
- **Typical Clients**: Web applications, mobile apps, SPAs

### Client Credentials Flow
- **Use Case**: Service-to-service communication
- **Token Contains**: Machine client identity (`azp` claim), no user identity
- **User Context**: Must be specified via `X-On-Behalf-Of` header
- **Typical Clients**: Backend services, scheduled jobs, integrations

## Implementation Details

### Token Detection
The system automatically detects the authentication type by examining JWT claims:
- **Grant Type (`gty`)**: `client-credentials` indicates machine authentication
- **Subject (`sub`)**: Present in user tokens, contains user ID
- **Authorized Party (`azp`)**: Identifies the client application

### User Context Resolution

#### For User Authentication
1. User authenticates via Auth0 Authorization Code flow
2. JWT token contains user identity in `sub` claim
3. System extracts user ID directly from token
4. All operations performed as the authenticated user

#### For Machine Authentication
1. Machine authenticates via Auth0 Client Credentials flow
2. JWT token contains machine identity, no user context
3. Machine must specify target user via `X-On-Behalf-Of` header
4. System validates machine has permission to act on behalf of user
5. Operations logged as "Machine X acting on behalf of User Y"

### Middleware Pipeline
1. **Authentication** (JWT validation)
2. **UserResolutionMiddleware** (determines user context)
3. **TenantResolutionMiddleware** (determines tenant context)
4. **Authorization** (built-in policy evaluation)
5. **PermissionAuthorizationMiddleware** (attribute-based permission validation)

## API Usage

### User Authentication Example
```http
GET /api/v0.1/learners/users
Authorization: Bearer {user_jwt_token}
X-Tenant-Id: {tenant_id}
```

### Machine Authentication Example
```http
GET /api/v0.1/learners/users
Authorization: Bearer {machine_jwt_token}
X-On-Behalf-Of: {user_id}
X-Tenant-Id: {tenant_id}
```

## Security Considerations

### Machine-to-Machine Permissions
- Machine clients require explicit permissions in Auth0
- All machine operations are logged with both machine and user identity
- Consider implementing additional authorization policies for sensitive operations

### Token Claims
- Custom claims are normalized during token validation
- Tenant associations validated against token claims
- Role claims extracted and mapped to ASP.NET Core identity

## Authorization System

### Unified Permission Model
The system uses a unified permission model that works seamlessly with both authentication types:

#### **Permission Structure**
- Format: `resource:action` (e.g., `users:read`, `tenants:manage`)
- Hierarchical: `manage` permissions include all specific actions
- Consistent across user roles and machine scopes

#### **Role to Permission Mapping**
```csharp
// User roles automatically map to permissions
"superadmin" → system:admin, tenants:manage, users:manage, learning:manage
"admin"      → users:manage, learning:manage
"instructor" → learning:create, learning:update, users:read
"learner"    → learning:read
```

#### **Machine Scopes**
```json
// Machine clients use direct scopes/permissions
{
  "scope": "users:read users:create tenants:manage"
}
```

### Controller Authorization

#### **Attribute-Based Authorization (Recommended)**
Use `RequirePermissionAttribute` for clean, declarative authorization:

```csharp
[Authorize] // Ensure authenticated
[RequirePermission(Permissions.Users.Read)]
public async Task<IActionResult> GetUsers()
{
    // Permission validation handled automatically by attribute
    // ... business logic only
}

// Multiple permissions (ANY logic - user needs at least one)
[RequirePermission(Permissions.Users.Read, Permissions.Users.Manage)]
public async Task<IActionResult> GetUserDetails(Guid id)
{
    // ... business logic
}

// Multiple permissions (ALL logic - user needs all permissions)
[RequirePermission(Permissions.Users.Update, Permissions.Users.Manage, RequireAll = true)]
public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequest request)
{
    // ... business logic
}
```

#### **Manual Permission Checks (Legacy)**
For complex scenarios, you can still use `IPermissionService` directly:

```csharp
[Authorize] // Just ensure authenticated  
public async Task<IActionResult> ComplexOperation()
{
    if (!await _permissionService.HasPermissionAsync(Permissions.Users.Read))
    {
        return StatusCode(403, "Insufficient permissions");
    }
    // ... business logic
}
```

#### **Benefits of Attribute-Based Authorization**
- **Cleaner Code**: Authorization logic separate from business logic
- **Compile-Time Safety**: Permission requirements visible in method signature
- **Consistent Error Handling**: Automatic 403 responses with detailed messages
- **Framework Integration**: Works with ASP.NET Core's authorization pipeline
- **Better Maintainability**: Permission changes only require attribute updates

### Permission Constants
Use the `Permissions` static class for type-safe permission checks:
- `Permissions.Users.Read/Create/Update/Delete/Manage`
- `Permissions.Tenants.Read/Create/Update/Delete/Manage`
- `Permissions.LearningObjects.Read/Create/Update/Delete/Manage`
- `Permissions.System.Admin`

## Technical Implementation

### Attribute-Based Authorization Architecture

The system uses a hybrid approach combining ASP.NET Core's authorization framework with custom middleware:

#### **Components**
1. **RequirePermissionAttribute**: Custom authorization attribute that extends `AuthorizeAttribute`
2. **PermissionPolicyProvider**: Dynamically creates authorization policies based on permission requirements
3. **DynamicPermissionAuthorizationHandler**: Validates dynamic policies against endpoint metadata
4. **PermissionAuthorizationMiddleware**: Processes permission attributes and validates permissions using `IPermissionService`

#### **Flow**
1. Controller method decorated with `[RequirePermission]` attribute
2. Attribute generates unique policy name based on permissions and requirement type
3. ASP.NET Core authorization evaluates dynamic policy
4. Policy provider creates policy with dynamic requirement
5. Authorization handler validates policy against endpoint metadata
6. Custom middleware processes permission attributes and calls `IPermissionService`
7. Consistent error responses returned for authorization failures

#### **Policy Naming**
Dynamic policies use the format: `Permission_{ANY|ALL}_{base64hash}`
- `Permission_ANY_dGVuYW50czpyZWFk` → Single permission: `tenants:read`  
- `Permission_ALL_dXNlcnM6cmVhZCx1c2Vyczp3cml0ZQ` → Multiple permissions requiring all

#### **Error Handling**
- Automatic 403 responses for insufficient permissions
- Detailed error messages including required permissions
- Consistent error format across all endpoints
- Proper logging of authorization failures

#### **Testing Support**
Integration tests can specify permissions directly:
```csharp
// In test setup
SetTestUserPermissions(Permissions.Users.Read, Permissions.Users.Create);

// Test will run with specified permissions
await Client.GetAsync("/api/v0.1/learners/users");
```

## Configuration

### Auth0 Setup

#### **For User Authentication**
1. Create Authorization Code application
2. Configure custom claims in Auth0 Rules/Actions:
   ```javascript
   // Add tenant associations and roles
   const namespace = 'https://your-api-identifier/';
   context.accessToken[namespace + 'tenant_id'] = user.app_metadata.tenant_id;
   context.accessToken[namespace + 'roles'] = user.app_metadata.roles || [];
   ```

#### **For Machine Authentication**
1. Create Machine-to-Machine application
2. Grant specific scopes (permissions):
   - `users:read`, `users:create`, `users:update`, `users:delete`
   - `tenants:read`, `tenants:create`, `tenants:update`, `tenants:delete`
   - Or broader scopes: `users:manage`, `tenants:manage`

#### **Custom Claims (Optional)**
For advanced scenarios, add permission claims directly:
```javascript
// In Auth0 Rule/Action
context.accessToken[namespace + 'permissions'] = [
  'users:read',
  'custom:permission'
];
```

### Application Settings
```json
{
  "Jwt": {
    "Authority": "https://your-domain.auth0.com/",
    "Audience": "your-api-identifier",
    "ValidateAudience": true,
    "ValidateIssuer": true
  }
}
```

## Migration Guide

### Upgrading from Manual Permission Checks

**Before (Legacy)**:
```csharp
[Authorize]
public async Task<IActionResult> GetUsers()
{
    if (!await _permissionService.HasPermissionAsync(Permissions.Users.Read))
    {
        var errorResponse = ErrorResponseDto.ForbiddenError(
            "Insufficient permissions",
            HttpContext.Items["CorrelationId"]?.ToString());
        return StatusCode(403, errorResponse);
    }
    
    // Business logic
    var users = await _mediator.Send(new GetAllUsersQuery());
    return Ok(users);
}
```

**After (Attribute-Based)**:
```csharp
[Authorize]
[RequirePermission(Permissions.Users.Read)]
public async Task<IActionResult> GetUsers()
{
    // Business logic only - authorization handled by attribute
    var users = await _mediator.Send(new GetAllUsersQuery());
    return Ok(users);
}
```

### Benefits of Migration
- **50% fewer lines of code** in controller methods
- **Consistent error responses** automatically generated
- **Compile-time visibility** of permission requirements  
- **Reduced maintenance** - single source of truth for permissions
- **Better testability** - permission requirements visible in method signature

## Troubleshooting

### Common Issues
- **Missing X-On-Behalf-Of Header**: Machine clients must specify target user
- **Permission Denied**: Check role mappings and machine scopes in Auth0
- **Invalid User Context**: Ensure user exists and machine has permissions
- **Token Type Mismatch**: Verify correct OAuth flow for your use case

### Permission Debugging
- Check available permissions: `await _permissionService.GetPermissionsAsync()`
- Validate specific permission: `await _permissionService.ValidatePermissionAsync("users:read")`
- Use hierarchical permissions: `users:manage` grants `users:read`, `users:create`, etc.

### Authorization Response Format
Failed permission checks return standardized error responses:
```json
{
  "title": "Forbidden",
  "status": 403,
  "detail": "Insufficient permissions. Required: ANY of [users:read]",
  "traceId": "correlation-id-here",
  "timestamp": "2025-09-03T10:00:00Z"
}
```

### Logging
- Authentication type logged for each request
- Machine-to-machine operations include both identities
- Failed authorization attempts logged with specific permission requirements
- Permission checks logged at Debug level
- Attribute-based authorization logs both successful and failed permission validations