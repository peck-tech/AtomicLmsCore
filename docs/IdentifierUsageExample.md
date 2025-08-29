# Identifier Usage Example

This document demonstrates how to properly use the hybrid ID approach in AtomicLMS Core.

## BaseEntity Structure

All entities inherit from `BaseEntity` which provides:
- `InternalId` (int) - Database primary key, auto-incremented, never exposed
- `Id` (Guid) - Public-facing sequential ULID-based GUID for API exposure

Sequential IDs provide:
- **Time-ordered**: ULIDs are lexicographically sortable by creation time
- **Performance**: Reduces database index fragmentation compared to random GUIDs
- **Security**: Still prevents enumeration attacks like random GUIDs
- **Distributed-friendly**: No coordination needed between multiple instances

## Example Entity

```csharp
// Domain/Entities/Course.cs
public class Course : BaseEntity
{
    public string Title { get; set; }
    public string Description { get; set; }
    // InternalId and Id are inherited from BaseEntity
}
```

## DTO Example (Correct Usage)

```csharp
// Application/Courses/CourseDto.cs
public class CourseDto
{
    public Guid Id { get; set; }  // ✅ Use public Guid Id
    // public int InternalId { get; set; }  // ❌ Never expose InternalId
    public string Title { get; set; }
    public string Description { get; set; }
}
```

## Repository Example

```csharp
// Infrastructure/Repositories/CourseRepository.cs
public class CourseRepository
{
    // Internal operations can use InternalId for performance
    private async Task<Course> GetByInternalIdAsync(int internalId)
    {
        return await _context.Courses
            .FirstOrDefaultAsync(c => c.InternalId == internalId);
    }

    // Public operations should use Guid Id
    public async Task<Course> GetByIdAsync(Guid id)
    {
        return await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
```

## API Controller Example

```csharp
// WebApi/Controllers/CoursesController.cs
[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    // ✅ Correct: Use Guid in route
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourseDto>> GetCourse(Guid id)
    {
        var course = await _courseService.GetByIdAsync(id);
        return Ok(_mapper.Map<CourseDto>(course));
    }

    // ❌ Wrong: Never expose InternalId in API
    // [HttpGet("{internalId:int}")]
    // public async Task<ActionResult<CourseDto>> GetCourse(int internalId)
}
```

## AutoMapper Configuration

```csharp
// Application/Mappings/CourseMappingProfile.cs
public class CourseMappingProfile : Profile
{
    public CourseMappingProfile()
    {
        CreateMap<Course, CourseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            // Never map InternalId to DTOs
            ;
    }
}
```

## Database Relationships

```csharp
// Use InternalId for foreign keys (database performance)
public class Enrollment : BaseEntity
{
    public int CourseInternalId { get; set; }  // Foreign key uses InternalId
    public Course Course { get; set; }
    
    public int StudentInternalId { get; set; }
    public Student Student { get; set; }
}

// EF Configuration
modelBuilder.Entity<Enrollment>()
    .HasOne(e => e.Course)
    .WithMany(c => c.Enrollments)
    .HasForeignKey(e => e.CourseInternalId)
    .HasPrincipalKey(c => c.InternalId);
```

## Key Benefits

1. **Performance**: Integer primary keys for fast database operations
2. **Security**: Guids prevent enumeration attacks
3. **Scalability**: Guids support distributed systems
4. **Flexibility**: Can expose either identifier based on use case

## Remember

- Always use `Id` (Guid) in public APIs, DTOs, and client-facing code
- Use `InternalId` only for database operations and never expose it
- Foreign key relationships should use `InternalId` for performance
- Entity Framework is configured to use `InternalId` as the primary key automatically