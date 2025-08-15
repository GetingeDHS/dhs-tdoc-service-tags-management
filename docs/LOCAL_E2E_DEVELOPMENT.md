# Local E2E Development Setup

This guide helps you set up a fast local development environment for E2E testing with Playwright, avoiding the slow PR-based Azure deployments during development iterations.

## 🚀 Quick Start

### 1. One-Time Setup

```powershell
# Set up fresh local environment with database
.\scripts\setup-local-e2e.ps1
```

This script will:
- ✅ Test SQL Server connection
- ✅ Create `TagManagement_Local_E2E` database
- ✅ Apply EF Core migrations
- ✅ Seed test data
- ✅ Start the API server

### 2. Run E2E Tests

```powershell
# In another terminal, run E2E tests
.\scripts\run-local-e2e-tests.ps1
```

## 📋 Prerequisites

- **SQL Server** (default instance on localhost)
- **Windows Authentication** enabled
- **.NET 8 SDK**
- **Playwright browsers** installed (`npx playwright install`)

## 🔧 Development Workflow

### Iterative Development

```powershell
# 1. Start fresh environment (drops/recreates DB)
.\scripts\setup-local-e2e.ps1 -Force

# 2. In another terminal, run tests with browser UI visible
.\scripts\run-local-e2e-tests.ps1 -Headed

# 3. Debug specific test with slow motion
.\scripts\run-local-e2e-tests.ps1 -Debug -Test "HealthCheck"

# 4. Make code changes and re-run tests
.\scripts\run-local-e2e-tests.ps1
```

### Visual Studio Debugging

```powershell
# Setup database without starting API
.\scripts\setup-local-e2e.ps1 -SkipApi

# Then debug the API in Visual Studio
# Run tests against your debugged instance
.\scripts\run-local-e2e-tests.ps1 -Headed
```

## 🗄️ Database Management

### Fresh Database
```powershell
# Forces complete database recreation
.\scripts\setup-local-e2e.ps1 -Force
```

### Manual Database Operations
```sql
-- Connect to localhost with Windows Auth
USE TagManagement_Local_E2E;

-- Check seeded data
SELECT * FROM TTAGS;
SELECT * FROM TTAGTYPE;
SELECT * FROM TUNIT;
SELECT * FROM TTAGCONTENT;
```

### Connection String
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TagManagement_Local_E2E;Integrated Security=true;TrustServerCertificate=true"
  }
}
```

## 🎭 Playwright Configuration

### Local Testing Configuration
The setup script automatically configures Playwright for local testing:

```json
{
  "TestSettings": {
    "BaseUrl": "https://localhost:7001",
    "Timeout": 30000,
    "BrowserType": "chromium",
    "Headless": false,
    "Screenshots": true
  }
}
```

### Test Modes

| Mode | Command | Description |
|------|---------|-------------|
| **Headless** | `.\scripts\run-local-e2e-tests.ps1` | Fast, no browser UI |
| **Headed** | `.\scripts\run-local-e2e-tests.ps1 -Headed` | Shows browser, good for debugging |
| **Debug** | `.\scripts\run-local-e2e-tests.ps1 -Debug -Headed` | Slow motion + browser UI |
| **Specific Test** | `.\scripts\run-local-e2e-tests.ps1 -Test "HealthCheck"` | Run only matching tests |

## 📊 Test Data Structure

The local environment automatically seeds the same test data as Azure E2E tests:

### TagTypes
- **PREP** (ID: 1) - Prep Tag
- **BUNDLE** (ID: 2) - Bundle Tag  
- **BASKET** (ID: 3) - Basket Tag
- **STERIL** (ID: 4) - Sterilization Load Tag

### Tags
- **Tag #1** (PREP type) - Contains Units 1 & 2
- **Tag #2** (BUNDLE type, auto) 
- **Tag #3** (BASKET type)

### Units
- **Unit #1** (TEST-UNIT-001) - In Test Location A
- **Unit #2** (TEST-UNIT-002) - In Test Location A

### Locations
- **Test Location A** (ID: 1)
- **Test Location B** (ID: 2)

## 🚨 Common Issues

### SQL Server Connection Failed
```powershell
# Check if SQL Server is running
Get-Service -Name "MSSQLSERVER"

# Start if stopped
Start-Service -Name "MSSQLSERVER"
```

### Permission Denied
```sql
-- Add your Windows user to SQL Server
-- Connect as admin and run:
ALTER SERVER ROLE sysadmin ADD MEMBER [DOMAIN\YourUsername]
```

### Port Already in Use
```powershell
# Find what's using port 7001
netstat -ano | findstr :7001

# Kill the process (replace PID)
taskkill /PID 1234 /F
```

### EF Migration Errors
```powershell
# Reset migrations if needed
dotnet ef database drop --project src/TagManagement.Infrastructure --startup-project src/TagManagement.Api
.\scripts\setup-local-e2e.ps1 -Force
```

## 🔄 Reset Everything
```powershell
# Nuclear option - completely fresh start
.\scripts\setup-local-e2e.ps1 -Force
```

## 💡 Pro Tips

1. **Fast Iteration**: Keep API running, only re-run test script
2. **Visual Debugging**: Use `-Headed -Debug` for new test development
3. **Selective Testing**: Use `-Test "pattern"` to focus on specific functionality
4. **Clean Slate**: Use `-Force` when schema changes or data gets corrupted
5. **VS Debugging**: Use `-SkipApi` then debug in Visual Studio for API issues

## 🌐 URLs

| Service | URL | Purpose |
|---------|-----|---------|
| **API** | https://localhost:7001 | Main API endpoint |
| **Health** | https://localhost:7001/health | Health check |
| **Swagger** | https://localhost:7001/swagger | API documentation |
| **Test Endpoints** | https://localhost:7001/api/test/* | Test-specific endpoints |

## ⚡ Performance

Local development is **significantly faster** than Azure deployments:

- **Azure E2E**: ~15 minutes (Terraform + deployment + tests)
- **Local E2E**: ~30 seconds (database + tests)

Perfect for rapid test development and debugging! 🚀
