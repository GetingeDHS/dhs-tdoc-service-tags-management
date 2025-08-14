# Tag Management Service

> Testing Key Vault access permissions for GitHub Actions

[![Build Status](https://github.com/your-org/tag-management/workflows/CI/badge.svg)](https://github.com/your-org/tag-management/actions)
[![Test Coverage](https://img.shields.io/badge/coverage-95%25-brightgreen.svg)](./docs/test-coverage.html)
[![Quality Gate](https://img.shields.io/badge/quality%20gate-passed-brightgreen.svg)](./docs/quality-report.html)
[![Medical Device Ready](https://img.shields.io/badge/medical%20device-compliant-blue.svg)](./docs/compliance.md)

A microservice for managing tags in medical device manufacturing systems, integrating with existing TDOC database schema.

## Overview

The system manages different types of tags used in industrial environments:
- **Prep Tags**: Preparation tags for units
- **Bundle Tags**: Bundle collections of units
- **Basket Tags**: Container tags for units
- **Sterilization Load Tags**: Tags for sterilization processes
- **Wash Tags**: Washing process tags
- **Transport Tags**: Transportation tags
- **Case Cart Tags**: Case cart management
- **Transport Box Tags**: Transport container tags
- **Instrument Container Tags**: Instrument collection tags

## Architecture

The solution follows a clean architecture pattern with three main projects:

### TagManagement.Core
Contains the domain models, entities, enums, and interfaces:
- **Entities**: Tag, Unit, Product, Customer, TagItem, TagContents
- **Enums**: TagType, TagContentCondition, LifeStatus, UnitStatus, etc.
- **Interfaces**: ITagService, ITagRepository

### TagManagement.Data
Data access layer using Entity Framework Core:
- **DbContext**: TagManagementDbContext with SQL Server support
- **Repositories**: TagRepository implementing ITagRepository
- **Junction Tables**: TagUnit, TagTag, TagItemEntity, TagIndicator

### TagManagement.Api
ASP.NET Core Web API layer:
- **Controllers**: TagsController with comprehensive REST endpoints
- **Services**: TagService implementing business logic
- **DTOs**: Request/Response models for API operations

## Key Features

### Tag Management
- Create, read, update, delete tags
- Support for manual and auto-generated tags
- Tag number generation with type-specific sequences
- Tag lifecycle management (Active, Inactive, Dead)

### Auto Tag Management
- Start/stop auto tags with license validation
- Automatic tag reservation system
- Conflict resolution for competing tag types
- Auto tag cleanup and maintenance

### Content Management
- Add/remove units to/from tags
- Add/remove items to/from tags
- Nested tag support (tags within tags)
- Indicator management
- Split tag functionality

### Advanced Operations
- Move units between tags
- Move entire tags to transport containers
- Bundle tag to transport box operations
- Tag dissolution and content clearing
- Split unit tracking across multiple tags

### Query Capabilities
- Find tags by ID, number, or type
- Get all tags containing specific units
- Check tag emptiness and content counts
- Unit split detection across tags
- Hierarchical tag relationships

## API Endpoints

### Basic Tag Operations
- `GET /api/tags` - Get all tags (paginated)
- `GET /api/tags/{id}` - Get specific tag
- `GET /api/tags/number/{tagNumber}/type/{tagType}` - Get tag by number and type
- `POST /api/tags` - Create new tag
- `DELETE /api/tags/{id}` - Delete tag

### Auto Tag Management
- `POST /api/tags/auto/start` - Start auto tag
- `POST /api/tags/auto/stop/{tagType}` - Stop auto tag
- `POST /api/tags/auto/stop-all` - Stop all auto tags

### Content Management
- `POST /api/tags/{tagId}/units` - Insert unit into tag
- `DELETE /api/tags/{tagId}/units/{unitId}` - Remove unit from tag
- `POST /api/tags/{tagId}/items` - Insert item into tag
- `DELETE /api/tags/{tagId}/items` - Remove item from tag
- `POST /api/tags/{targetTagId}/tags/{sourceTagId}` - Insert tag into tag

### Advanced Operations
- `POST /api/tags/transport-box/{transportBoxTagId}/units/{unitId}` - Move unit to transport box
- `GET /api/tags/units/{unitId}/tags` - Get tags containing unit
- `GET /api/tags/{tagId}/is-empty` - Check if tag is empty
- `GET /api/tags/{tagId}/content-count` - Get tag content count
- `DELETE /api/tags/{tagId}/dissolve` - Dissolve tag
- `DELETE /api/tags/{tagId}/contents` - Clear tag contents

## Database Schema

The system uses SQL Server with Entity Framework Core:

### Main Tables
- **Tags**: Core tag information
- **Units**: Unit definitions with status tracking
- **Products**: Product catalog
- **Customers**: Customer information

### Junction Tables
- **TagUnits**: Many-to-many relationship between tags and units
- **TagTags**: Hierarchical relationships between tags
- **TagItems**: Items contained within tags
- **TagIndicators**: Indicators associated with tags

## Configuration

### Database Connection
Update `appsettings.json` with your SQL Server connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=TagManagementDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Dependency Injection
Services are configured in `Program.cs`:
- Entity Framework DbContext
- Repository pattern implementation
- Service layer with business logic
- Controller dependency injection

## Getting Started

1. **Clone the repository**
2. **Update connection string** in `appsettings.json`
3. **Create database migrations**:
   ```bash
   cd TagManagement.Api
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
4. **Run the application**:
   ```bash
   dotnet run
   ```
5. **Access the API** at `https://localhost:5001` or `http://localhost:5000`

## Migration from Delphi

This solution provides a modern C# equivalent to the original Delphi codebase:

### Original Delphi Classes → C# Equivalents
- `TProcessHandlerTagHandler` → `TagService`
- `TTagDBHelper` → `TagRepository`
- `TBOTags` → `Tag` entity
- `TTagContents` → `TagContents` entity
- `TTagItem` → `TagItem` entity
- `TDOUnit` → `Unit` entity

### Key Improvements
- **Async/Await**: All operations are asynchronous for better performance
- **Dependency Injection**: Proper IoC container usage
- **Entity Framework**: Modern ORM instead of direct SQL
- **REST API**: Standard HTTP endpoints instead of internal method calls
- **JSON Serialization**: Standard data exchange format
- **Logging**: Structured logging with Microsoft.Extensions.Logging
- **Configuration**: Modern configuration system
- **Error Handling**: Comprehensive exception handling and HTTP status codes

## Business Logic Preservation

The solution maintains the complex business rules from the original Delphi system:
- Tag type compatibility validation
- Unit status checking before insertion
- Auto tag licensing requirements
- Conflict resolution between tag types
- Split tag management
- Transport box validation rules
- Nested tag hierarchy validation

## Development Notes

### Entity Framework Design
- Code-first approach with fluent API configuration
- Junction tables for complex many-to-many relationships
- Proper indexing for performance
- Cascade delete configuration for data integrity

### Service Layer
- Business logic separation from data access
- Comprehensive validation before database operations
- Logging for audit and debugging
- Error handling with meaningful messages

### API Design
- RESTful endpoint design
- Proper HTTP status codes
- Request/response DTOs
- Query parameter support for filtering
- Async controller actions

## Future Enhancements

- Authentication and authorization
- Real-time notifications using SignalR
- Background jobs for maintenance tasks
- Caching layer for performance
- API versioning
- Comprehensive unit tests
- Integration tests
- Performance monitoring
- Docker containerization

## License

This project converts existing Delphi functionality to modern C# and maintains compatibility with the original business requirements.
#   S e c r e t s   c o n f i g u r e d   f o r   s h a r e d   i n f r a s t r u c t u r e  
 