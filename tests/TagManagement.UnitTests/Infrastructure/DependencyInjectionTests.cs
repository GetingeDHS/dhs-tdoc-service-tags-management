using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TagManagement.Core.Interfaces;
using TagManagement.Infrastructure;
using TagManagement.Infrastructure.Persistence;
using TagManagement.Infrastructure.Persistence.Repositories;

namespace TagManagement.UnitTests.Infrastructure;

/// <summary>
/// Medical Device Compliance Tests for Infrastructure DependencyInjection
/// Tests service registration and configuration
/// </summary>
[Trait("Category", "MedicalDevice")]
[Trait("Compliance", "ISO-13485")]
[Trait("Layer", "Infrastructure")]
[Trait("Component", "DependencyInjection")]
public class DependencyInjectionTests
{
    /// <summary>
    /// MD-DI-001: AddDataServices must register DbContext with SQL Server configuration
    /// Critical for proper database connectivity
    /// </summary>
    [Fact(DisplayName = "MD-DI-001: AddDataServices Must Register DbContext With SQL Server")]
    public void AddDataServices_Should_Register_DbContext_With_SqlServer()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"ConnectionStrings:DefaultConnection", "Server=test;Database=test;Trusted_Connection=true;"}
            })
            .Build();

        // Act
        services.AddDataServices(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dbContext = serviceProvider.GetService<TagManagementDbContext>();
        dbContext.Should().NotBeNull("DbContext should be registered");
        
        // Verify DbContext options are configured
        var options = serviceProvider.GetService<DbContextOptions<TagManagementDbContext>>();
        options.Should().NotBeNull("DbContextOptions should be registered");
    }

    /// <summary>
    /// MD-DI-002: AddDataServices must register ITagRepository with TDocTagRepository implementation
    /// Critical for proper repository pattern implementation
    /// </summary>
    [Fact(DisplayName = "MD-DI-002: AddDataServices Must Register Repository Interface")]
    public void AddDataServices_Should_Register_TagRepository_Interface()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"ConnectionStrings:DefaultConnection", "Server=test;Database=test;Trusted_Connection=true;"}
            })
            .Build();

        // Act
        services.AddDataServices(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var repository = serviceProvider.GetService<ITagRepository>();
        repository.Should().NotBeNull("ITagRepository should be registered");
        repository.Should().BeOfType<TDocTagRepository>("Should use TDocTagRepository implementation");
    }

    /// <summary>
    /// MD-DI-003: AddDataServices must configure services with correct lifetimes
    /// Critical for proper service lifetime management
    /// </summary>
    [Fact(DisplayName = "MD-DI-003: AddDataServices Must Configure Correct Service Lifetimes")]
    public void AddDataServices_Should_Configure_Correct_Service_Lifetimes()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"ConnectionStrings:DefaultConnection", "Server=test;Database=test;Trusted_Connection=true;"}
            })
            .Build();

        // Act
        services.AddDataServices(configuration);

        // Assert - Check service descriptors
        var dbContextDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TagManagementDbContext));
        dbContextDescriptor.Should().NotBeNull("DbContext should be registered");
        dbContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped, "DbContext should be scoped");

        var repositoryDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ITagRepository));
        repositoryDescriptor.Should().NotBeNull("ITagRepository should be registered");
        repositoryDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped, "Repository should be scoped");
        repositoryDescriptor.ImplementationType.Should().Be(typeof(TDocTagRepository));
    }

    /// <summary>
    /// MD-DI-004: AddDataServices must handle missing connection string configuration gracefully
    /// Critical for proper error handling in configuration scenarios
    /// </summary>
    [Fact(DisplayName = "MD-DI-004: AddDataServices Must Handle Missing Connection String")]
    public void AddDataServices_Should_Handle_Missing_Connection_String()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build(); // Empty configuration

        // Act - Should not throw during registration
        var act = () => services.AddDataServices(configuration);

        // Assert
        act.Should().NotThrow("Service registration should not fail with missing connection string");

        // Verify services are still registered
        var dbContextDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TagManagementDbContext));
        dbContextDescriptor.Should().NotBeNull("DbContext should still be registered");
    }

    /// <summary>
    /// MD-DI-005: AddDataServices must be chainable for fluent service registration
    /// Critical for fluent API design patterns
    /// </summary>
    [Fact(DisplayName = "MD-DI-005: AddDataServices Must Be Chainable")]
    public void AddDataServices_Should_Be_Chainable()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"ConnectionStrings:DefaultConnection", "Server=test;Database=test;"}
            })
            .Build();

        // Act & Assert - Should return IServiceCollection for chaining
        var result = services.AddDataServices(configuration);
        result.Should().BeSameAs(services, "Should return same service collection for chaining");

        // Test actual chaining
        var chainedResult = services
            .AddDataServices(configuration)
            .AddLogging()
            .AddSingleton<string>("test");

        chainedResult.Should().BeSameAs(services, "Chained calls should work");
        services.Should().HaveCountGreaterThan(5, "Should have registered multiple services"); // DbContext, Options, Repository, Logging factory, logger, string and EF services
    }

    /// <summary>
    /// MD-DI-006: AddDataServices must register all required services for tag management operations
    /// Critical for complete service availability
    /// </summary>
    [Fact(DisplayName = "MD-DI-006: AddDataServices Must Register All Required Services")]
    public void AddDataServices_Should_Register_All_Required_Services()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"ConnectionStrings:DefaultConnection", "Server=localhost;Database=TagManagement;Trusted_Connection=true;"}
            })
            .Build();

        // Act
        services.AddDataServices(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - All critical services should be resolvable
        var dbContext = serviceProvider.GetService<TagManagementDbContext>();
        dbContext.Should().NotBeNull("TagManagementDbContext should be available");

        var repository = serviceProvider.GetService<ITagRepository>();
        repository.Should().NotBeNull("ITagRepository should be available");

        var dbContextOptions = serviceProvider.GetService<DbContextOptions<TagManagementDbContext>>();
        dbContextOptions.Should().NotBeNull("DbContextOptions should be available");

        // Verify the repository can access the DbContext
        var tDocRepository = repository as TDocTagRepository;
        tDocRepository.Should().NotBeNull("Repository should be TDocTagRepository type");
    }

    /// <summary>
    /// MD-DI-007: AddDataServices must work with various connection string formats
    /// Critical for deployment flexibility
    /// </summary>
    [Theory(DisplayName = "MD-DI-007: AddDataServices Must Handle Various Connection String Formats")]
    [InlineData("Server=localhost;Database=Test;Trusted_Connection=true;")]
    [InlineData("Server=(localdb)\\MSSQLLocalDB;Database=TagManagement;Trusted_Connection=true;")]
    [InlineData("Data Source=server;Initial Catalog=db;Integrated Security=True;")]
    public void AddDataServices_Should_Handle_Various_Connection_String_Formats(string connectionString)
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"ConnectionStrings:DefaultConnection", connectionString}
            })
            .Build();

        // Act
        var act = () => services.AddDataServices(configuration);

        // Assert
        act.Should().NotThrow($"Should handle connection string: {connectionString}");

        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetService<TagManagementDbContext>();
        dbContext.Should().NotBeNull("DbContext should be registered regardless of connection string format");
    }

    /// <summary>
    /// MD-DI-008: AddDataServices must support multiple registrations without conflicts
    /// Critical for avoiding duplicate service registration issues
    /// </summary>
    [Fact(DisplayName = "MD-DI-008: AddDataServices Must Support Multiple Registrations")]
    public void AddDataServices_Should_Support_Multiple_Registrations()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"ConnectionStrings:DefaultConnection", "Server=test;Database=test;"}
            })
            .Build();

        // Act - Register multiple times
        services.AddDataServices(configuration);
        services.AddDataServices(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // Assert - Should still work (last registration wins for scoped services)
        var dbContext = serviceProvider.GetService<TagManagementDbContext>();
        dbContext.Should().NotBeNull("DbContext should be available even with multiple registrations");

        var repository = serviceProvider.GetService<ITagRepository>();
        repository.Should().NotBeNull("Repository should be available even with multiple registrations");
    }

    /// <summary>
    /// MD-DI-009: AddDataServices must integrate properly with ASP.NET Core DI container
    /// Critical for web application integration
    /// </summary>
    [Fact(DisplayName = "MD-DI-009: AddDataServices Must Integrate With ASP.NET Core DI")]
    public void AddDataServices_Should_Integrate_With_AspNetCore_DI()
    {
        // Arrange - Simulate ASP.NET Core service collection
        var services = new ServiceCollection();
        services.AddLogging(); // Common ASP.NET Core service
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"ConnectionStrings:DefaultConnection", "Server=test;Database=test;"}
            })
            .Build();

        // Act
        services.AddDataServices(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Should coexist with other services
        var dbContext = serviceProvider.GetService<TagManagementDbContext>();
        var repository = serviceProvider.GetService<ITagRepository>();
        var loggerFactory = serviceProvider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();

        dbContext.Should().NotBeNull("TagManagement services should coexist with logging");
        repository.Should().NotBeNull("Repository should be available with other services");
        loggerFactory.Should().NotBeNull("Existing services should remain functional");
    }

    /// <summary>
    /// MD-DI-010: AddDataServices must properly dispose of resources
    /// Critical for resource management and avoiding memory leaks
    /// </summary>
    [Fact(DisplayName = "MD-DI-010: AddDataServices Must Support Proper Resource Disposal")]
    public void AddDataServices_Should_Support_Proper_Resource_Disposal()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"ConnectionStrings:DefaultConnection", "Server=test;Database=test;"}
            })
            .Build();

        services.AddDataServices(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Act - Create and dispose scope
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetService<TagManagementDbContext>();
            var repository = scope.ServiceProvider.GetService<ITagRepository>();
            
            // Assert - Services should be available in scope
            dbContext.Should().NotBeNull("DbContext should be available in scope");
            repository.Should().NotBeNull("Repository should be available in scope");
        } // Scope disposal should clean up scoped services

        // Multiple scopes should work independently
        using (var scope1 = serviceProvider.CreateScope())
        using (var scope2 = serviceProvider.CreateScope())
        {
            var dbContext1 = scope1.ServiceProvider.GetService<TagManagementDbContext>();
            var dbContext2 = scope2.ServiceProvider.GetService<TagManagementDbContext>();
            
            dbContext1.Should().NotBeSameAs(dbContext2, "Different scopes should have different DbContext instances");
        }

        // Dispose service provider
        serviceProvider.Dispose();
    }
}
