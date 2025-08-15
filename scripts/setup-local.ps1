param([switch]$Force)

$ErrorActionPreference = "Stop"
Write-Host "Setting up local E2E development environment..." -ForegroundColor Green

$DatabaseName = "TagManagement_Local_E2E"

# Test SQL connection
Write-Host "Testing SQL Server connection..." -ForegroundColor Yellow
try {
    $conn = "Server=localhost;Database=master;Integrated Security=true;TrustServerCertificate=true"
    $sql = New-Object System.Data.SqlClient.SqlConnection($conn)
    $sql.Open()
    $sql.Close()
    Write-Host "SQL Server connection successful" -ForegroundColor Green
}
catch {
    Write-Host "Cannot connect to SQL Server: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please ensure:" -ForegroundColor Red
    Write-Host "  1. SQL Server is running" -ForegroundColor Red
    Write-Host "  2. Windows Authentication is enabled" -ForegroundColor Red
    Write-Host "  3. Your user has appropriate permissions" -ForegroundColor Red
    exit 1
}

# Check if database exists
Write-Host "Checking if database exists..." -ForegroundColor Yellow
$checkCmd = "SELECT COUNT(*) FROM sys.databases WHERE name = '$DatabaseName'"
$sql = New-Object System.Data.SqlClient.SqlConnection($conn)
$sql.Open()
$cmd = $sql.CreateCommand()
$cmd.CommandText = $checkCmd
$exists = [int]$cmd.ExecuteScalar() -gt 0
$sql.Close()

if ($exists -and $Force) {
    Write-Host "Dropping existing database (Force mode)..." -ForegroundColor Yellow
    $sql = New-Object System.Data.SqlClient.SqlConnection($conn)
    $sql.Open()
    $cmd = $sql.CreateCommand()
    $cmd.CommandText = "ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
    $cmd.ExecuteNonQuery()
    $cmd.CommandText = "DROP DATABASE [$DatabaseName]"
    $cmd.ExecuteNonQuery()
    $sql.Close()
    Write-Host "Database dropped successfully" -ForegroundColor Green
    $exists = $false
}

if (-not $exists) {
    Write-Host "Creating database '$DatabaseName'..." -ForegroundColor Yellow
    $sql = New-Object System.Data.SqlClient.SqlConnection($conn)
    $sql.Open()
    $cmd = $sql.CreateCommand()
    $cmd.CommandText = "CREATE DATABASE [$DatabaseName]"
    $cmd.ExecuteNonQuery()
    $sql.Close()
    Write-Host "Database created successfully" -ForegroundColor Green
}

# Set environment variables for proper globalization
$env:DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = "false"
$env:LC_ALL = "en_US.UTF-8"
$env:ASPNETCORE_ENVIRONMENT = "Development"

# Run migrations
Write-Host "Applying EF Core migrations..." -ForegroundColor Yellow
dotnet ef database update --project src/TagManagement.Infrastructure --startup-project src/TagManagement.Api --configuration Debug --connection "Server=localhost;Database=$DatabaseName;Integrated Security=true;TrustServerCertificate=true"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Migrations applied successfully" -ForegroundColor Green
    Write-Host ""
    Write-Host "Local E2E environment is ready!" -ForegroundColor Green
    Write-Host "Database: $DatabaseName" -ForegroundColor White
    Write-Host "Connection: localhost (Windows Auth)" -ForegroundColor White
    Write-Host ""
    Write-Host "You can now:" -ForegroundColor Cyan
    Write-Host "  1. Run: dotnet run --project src/TagManagement.Api" -ForegroundColor White
    Write-Host "  2. Test at: https://localhost:7001/health" -ForegroundColor White
    Write-Host "  3. View Swagger: https://localhost:7001/swagger" -ForegroundColor White
} else {
    Write-Host "Migration failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit 1
}
