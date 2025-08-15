param([switch]$Force)

$ErrorActionPreference = "Stop"
Write-Host "🚀 Simple E2E setup..." -ForegroundColor Green

$DatabaseName = "TagManagement_Local_E2E"

# Test SQL connection
Write-Host "📡 Testing SQL Server..." -ForegroundColor Yellow
try {
    $conn = "Server=localhost;Database=master;Integrated Security=true;TrustServerCertificate=true"
    $sql = New-Object System.Data.SqlClient.SqlConnection($conn)
    $sql.Open()
    $sql.Close()
    Write-Host "✅ SQL Server OK" -ForegroundColor Green
}
catch {
    Write-Host "❌ SQL Server failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Check if database exists
Write-Host "🔍 Checking database..." -ForegroundColor Yellow
$checkCmd = "SELECT COUNT(*) FROM sys.databases WHERE name = '$DatabaseName'"
$sql = New-Object System.Data.SqlClient.SqlConnection($conn)
$sql.Open()
$cmd = $sql.CreateCommand()
$cmd.CommandText = $checkCmd
$exists = [int]$cmd.ExecuteScalar() -gt 0
$sql.Close()

if ($exists -and $Force) {
    Write-Host "🗑️ Dropping database..." -ForegroundColor Yellow
    $sql = New-Object System.Data.SqlClient.SqlConnection($conn)
    $sql.Open()
    $cmd = $sql.CreateCommand()
    $cmd.CommandText = "ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
    $cmd.ExecuteNonQuery()
    $cmd.CommandText = "DROP DATABASE [$DatabaseName]"
    $cmd.ExecuteNonQuery()
    $sql.Close()
    $exists = $false
}

if (-not $exists) {
    Write-Host "📊 Creating database..." -ForegroundColor Yellow
    $sql = New-Object System.Data.SqlClient.SqlConnection($conn)
    $sql.Open()
    $cmd = $sql.CreateCommand()
    $cmd.CommandText = "CREATE DATABASE [$DatabaseName]"
    $cmd.ExecuteNonQuery()
    $sql.Close()
}

# Run migrations
Write-Host "🔄 Running migrations..." -ForegroundColor Yellow
dotnet ef database update --project src/TagManagement.Infrastructure --startup-project src/TagManagement.Api

Write-Host "✅ Setup complete!" -ForegroundColor Green
