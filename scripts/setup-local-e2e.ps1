#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Sets up local development environment for E2E testing
    
.DESCRIPTION
    This script:
    1. Creates a fresh local database
    2. Applies EF Core migrations
    3. Seeds test data
    4. Starts the API for local E2E testing
    
.PARAMETER Force
    Forces recreation of database even if it exists
    
.PARAMETER SkipApi
    Skip starting the API (useful if you want to debug in VS)
#>

param(
    [switch]$Force,
    [switch]$SkipApi
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 Setting up local E2E development environment..." -ForegroundColor Green

# Configuration
$DatabaseName = "TagManagement_Local_E2E"
$ConnectionString = "Server=localhost;Database=$DatabaseName;Integrated Security=true;TrustServerCertificate=true"
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$ApiProject = Join-Path $ProjectRoot "src/TagManagement.Api/TagManagement.Api.csproj"

# Test SQL Server connection
Write-Host "📡 Testing SQL Server connection..." -ForegroundColor Yellow
try {
    $TestConnection = "Server=localhost;Database=master;Integrated Security=true;TrustServerCertificate=true"
    $Connection = New-Object System.Data.SqlClient.SqlConnection($TestConnection)
    $Connection.Open()
    $Connection.Close()
    Write-Host "✅ SQL Server connection successful" -ForegroundColor Green
} catch {
    Write-Host "❌ Cannot connect to SQL Server. Please ensure:" -ForegroundColor Red
    Write-Host "   1. SQL Server is running" -ForegroundColor Red
    Write-Host "   2. Windows Authentication is enabled" -ForegroundColor Red
    Write-Host "   3. Your user has appropriate permissions" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Check if database exists
Write-Host "🔍 Checking if database exists..." -ForegroundColor Yellow
$CheckDbQuery = "SELECT COUNT(*) FROM sys.databases WHERE name = '$DatabaseName'"
try {
    $Connection = New-Object System.Data.SqlClient.SqlConnection($TestConnection)
    $Connection.Open()
    $Command = $Connection.CreateCommand()
    $Command.CommandText = $CheckDbQuery
    $DbExists = [int]$Command.ExecuteScalar() -gt 0
    $Connection.Close()
} catch {
    Write-Host "❌ Error checking database existence: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Drop and recreate database if it exists and Force is specified
if ($DbExists) {
    if ($Force) {
        Write-Host "🗑️ Dropping existing database (Force mode)..." -ForegroundColor Yellow
        try {
            $Connection = New-Object System.Data.SqlClient.SqlConnection($TestConnection)
            $Connection.Open()
            $Command = $Connection.CreateCommand()
            # Close all connections to the database first
            $Command.CommandText = "ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
            $Command.ExecuteNonQuery()
            # Drop the database
            $Command.CommandText = "DROP DATABASE [$DatabaseName]"
            $Command.ExecuteNonQuery()
            $Connection.Close()
            Write-Host "✅ Database dropped successfully" -ForegroundColor Green
            $DbExists = $false
        } catch {
            Write-Host "❌ Error dropping database: $($_.Exception.Message)" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "📄 Database already exists. Use -Force to recreate it." -ForegroundColor Yellow
        Write-Host "   Current database will be used with existing data." -ForegroundColor Yellow
    }
}

# Create database if it doesn't exist
if (-not $DbExists) {
    Write-Host "📊 Creating database '$DatabaseName'..." -ForegroundColor Yellow
    try {
        $Connection = New-Object System.Data.SqlClient.SqlConnection($TestConnection)
        $Connection.Open()
        $Command = $Connection.CreateCommand()
        $Command.CommandText = "CREATE DATABASE [$DatabaseName]"
        $Command.ExecuteNonQuery()
        $Connection.Close()
        Write-Host "✅ Database created successfully" -ForegroundColor Green
    } catch {
        Write-Host "❌ Error creating database: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

# Set environment to use local config
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:DOTNET_ENVIRONMENT = "Development"

# Apply EF Core migrations
Write-Host "🔄 Applying EF Core migrations..." -ForegroundColor Yellow
try {
    Push-Location $ProjectRoot
    dotnet ef database update --project src/TagManagement.Infrastructure --startup-project src/TagManagement.Api --configuration Debug --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        throw "EF migration failed with exit code $LASTEXITCODE"
    }
    Write-Host "✅ Migrations applied successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Error applying migrations: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}

Write-Host "🎯 Local E2E environment is ready!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Environment Details:" -ForegroundColor Cyan
Write-Host "   Database: $DatabaseName" -ForegroundColor White
Write-Host "   Connection: localhost (Windows Auth)" -ForegroundColor White
Write-Host "   Config: appsettings.Development.Local.json" -ForegroundColor White
Write-Host ""

if (-not $SkipApi) {
    Write-Host "🚀 Starting API server..." -ForegroundColor Yellow
    Write-Host "   API will be available at: https://localhost:7001" -ForegroundColor White
    Write-Host "   Health check: https://localhost:7001/health" -ForegroundColor White
    Write-Host "   Swagger: https://localhost:7001/swagger" -ForegroundColor White
    Write-Host ""
    Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
    Write-Host ""
    
    try {
        Push-Location $ProjectRoot
        dotnet run --project $ApiProject --configuration Debug
    } finally {
        Pop-Location
    }
} else {
    Write-Host "✅ Setup complete! You can now:" -ForegroundColor Green
    Write-Host "   1. Debug the API in Visual Studio" -ForegroundColor White
    Write-Host "   2. Run: dotnet run --project src/TagManagement.Api" -ForegroundColor White
    Write-Host "   3. Run Playwright tests against localhost" -ForegroundColor White
    Write-Host ""
    Write-Host "💡 Tip: Use -Force to get a completely fresh database next time" -ForegroundColor Cyan
}
