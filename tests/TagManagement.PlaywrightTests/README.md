# TagManagement Playwright E2E Tests

This project contains comprehensive end-to-end (E2E) tests for the Tag Management service using Microsoft Playwright with .NET. The tests are designed to validate the complete user journey and API functionality in a real browser environment.

## Features

- **Comprehensive API Testing**: Tests cover all CRUD operations for tags, health checks, and API documentation
- **Azure Integration**: Tests run against a dedicated Azure test environment deployed specifically for each pull request
- **Medical Device Compliance**: Includes specific tests for audit trails, data integrity, and regulatory requirements
- **Rich Reporting**: Generates HTML reports, screenshots, and videos for failed tests
- **CI/CD Integration**: Automatically runs on pull requests with proper cleanup

## Test Categories

### API Tests
- Health endpoint validation
- Swagger documentation accessibility
- Tag CRUD operations (Create, Read, Update, Delete)
- Error handling and validation
- Response format verification

### Medical Device Compliance Tests
- Audit trail verification
- Data integrity constraints
- User access controls
- Regulatory reporting functions

## Configuration

Tests can be configured via `appsettings.json`:

```json
{
  "TestSettings": {
    "BaseUrl": "https://localhost:7001",
    "AzureTestUrl": "",
    "Timeout": 30000,
    "BrowserType": "chromium",
    "Headless": true,
    "VideoRecording": true,
    "Screenshots": true
  }
}
```

## Running Tests Locally

### Prerequisites

1. .NET 8.0 SDK
2. Playwright browsers installed

### Setup

```bash
# Restore packages
dotnet restore tests/TagManagement.PlaywrightTests/TagManagement.PlaywrightTests.csproj

# Build the project
dotnet build tests/TagManagement.PlaywrightTests/TagManagement.PlaywrightTests.csproj

# Install Playwright browsers
pwsh tests/TagManagement.PlaywrightTests/bin/Debug/net8.0/playwright.ps1 install
```

### Running Tests

```bash
# Run all tests
dotnet test tests/TagManagement.PlaywrightTests/TagManagement.PlaywrightTests.csproj

# Run specific category
dotnet test tests/TagManagement.PlaywrightTests/TagManagement.PlaywrightTests.csproj --filter "Category=API"

# Run with verbose output
dotnet test tests/TagManagement.PlaywrightTests/TagManagement.PlaywrightTests.csproj --verbosity detailed
```

## Azure Test Environment

The tests automatically deploy to a dedicated Azure environment for each pull request:

- **App Service**: Hosts the Tag Management API
- **SQL Database**: Dedicated test database with clean state
- **Application Insights**: Monitoring and logging
- **Key Vault**: Secure secret management

The environment is automatically created before tests run and destroyed after completion to ensure cost efficiency and isolation.

## Test Reports

Multiple report formats are generated:

1. **HTML Report**: Rich interactive report with screenshots and videos
2. **JUnit XML**: For CI/CD integration
3. **TRX Report**: Visual Studio compatible format
4. **Screenshots**: Captured on test failures
5. **Videos**: Recording of failed test sessions

## Medical Device Compliance

These tests are designed to support FDA and ISO-13485 compliance requirements:

- **Traceability**: Each test links to specific requirements
- **Validation**: Comprehensive validation of business rules
- **Audit Trail**: Verification of all changes are logged
- **Data Integrity**: Ensures data consistency and validation
- **Access Control**: Validates proper authentication and authorization

## CI/CD Integration

Tests are integrated with GitHub Actions:

- **Pull Request Trigger**: Automatically runs on PR creation/updates
- **Azure Deployment**: Creates isolated test environment
- **Parallel Execution**: Tests run efficiently in parallel
- **Automatic Cleanup**: Environment is destroyed after tests complete
- **PR Comments**: Results are posted directly to pull requests

## Debugging Failed Tests

When tests fail:

1. Check the GitHub Actions workflow logs
2. Download test artifacts (screenshots, videos, reports)
3. Review the HTML report for detailed failure information
4. Check Application Insights for server-side logs

## Best Practices

- Tests are designed to be independent and can run in any order
- Each test cleans up its own test data
- Tests use realistic test data that mimics production scenarios
- Assertions are clear and provide meaningful error messages
- Page object pattern is used for maintainable test code

## Extending Tests

To add new tests:

1. Create test methods in the appropriate test class
2. Use the `TestBase` class for common functionality
3. Add appropriate categories and descriptions
4. Ensure tests follow medical device compliance patterns
5. Update this README if adding new test categories

## Troubleshooting

Common issues and solutions:

### Browser Installation Issues
```bash
# Reinstall browsers
pwsh tests/TagManagement.PlaywrightTests/bin/Debug/net8.0/playwright.ps1 install --force
```

### Timeout Issues
- Increase timeout in `appsettings.json`
- Check network connectivity to test environment
- Verify Azure services are running properly

### Authentication Issues
- Ensure Azure credentials are properly configured
- Check service principal permissions
- Verify Key Vault access policies

## Support

For questions or issues with the E2E tests:

1. Check this documentation
2. Review GitHub Issues
3. Check workflow logs for specific error messages
4. Contact the development team for assistance
