#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Runs Playwright E2E tests against local development environment
    
.DESCRIPTION
    This script runs the Playwright E2E tests against your local API instance.
    Make sure the API is running on https://localhost:7001 before running this.
    
.PARAMETER Headed
    Run tests in headed mode (shows browser UI)
    
.PARAMETER Debug
    Run tests in debug mode with slow motion
    
.PARAMETER Test
    Run a specific test by name pattern
#>

param(
    [switch]$Headed,
    [switch]$Debug,
    [string]$Test = ""
)

$ErrorActionPreference = "Stop"

Write-Host "🎭 Running Playwright E2E tests against local environment..." -ForegroundColor Green

$ProjectRoot = Split-Path -Parent $PSScriptRoot
$PlaywrightProject = Join-Path $ProjectRoot "tests/TagManagement.PlaywrightTests"

# Check if API is running
Write-Host "📡 Checking if local API is running..." -ForegroundColor Yellow
try {
    $Response = Invoke-RestMethod -Uri "https://localhost:7001/health" -SkipCertificateCheck -TimeoutSec 5
    if ($Response.status -eq "Healthy") {
        Write-Host "✅ API is running and healthy" -ForegroundColor Green
    } else {
        Write-Host "⚠️ API responded but status is: $($Response.status)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Cannot reach local API at https://localhost:7001" -ForegroundColor Red
    Write-Host "   Please start the API first using:" -ForegroundColor Red
    Write-Host "   .\scripts\setup-local-e2e.ps1" -ForegroundColor Red
    Write-Host "   OR" -ForegroundColor Red
    Write-Host "   dotnet run --project src/TagManagement.Api" -ForegroundColor Red
    exit 1
}

# Configure Playwright for local testing
Write-Host "⚙️ Configuring Playwright for local testing..." -ForegroundColor Yellow

$LocalConfig = @{
    "TestSettings" = @{
        "BaseUrl" = "https://localhost:7001"
        "AzureTestUrl" = ""
        "Timeout" = 30000
        "BrowserType" = "chromium"
        "Headless" = -not $Headed
        "SlowMo" = if ($Debug) { 1000 } else { 0 }
        "VideoRecording" = $true
        "Screenshots" = $true
    }
    "TestData" = @{
        "ValidTestUser" = @{
            "Username" = "testuser@example.com"
            "Password" = "TestPassword123!"
        }
        "TestTags" = @{
            "DefaultTagType" = "Equipment"
            "TestUnitName" = "Test Unit 001"
            "TestLocationName" = "Test Location A"
        }
    }
} | ConvertTo-Json -Depth 3

$ConfigPath = Join-Path $PlaywrightProject "appsettings.json"
$LocalConfig | Out-File -FilePath $ConfigPath -Encoding UTF8

Write-Host "✅ Playwright configured for local testing" -ForegroundColor Green

# Build test arguments
$TestArgs = @(
    "test"
    $PlaywrightProject
    "--configuration", "Debug"
    "--logger", "console;verbosity=normal"
    "--settings", $ConfigPath
)

if ($Test) {
    $TestArgs += "--filter", "Name~$Test"
    Write-Host "🎯 Running specific test pattern: $Test" -ForegroundColor Cyan
} else {
    Write-Host "🎯 Running all E2E tests" -ForegroundColor Cyan
}

# Set environment variables for Playwright
$env:PLAYWRIGHT_BROWSERS_PATH = "0"  # Use system browsers
if ($Headed) {
    Write-Host "👁️ Running in headed mode (browser UI visible)" -ForegroundColor Cyan
}
if ($Debug) {
    Write-Host "🐛 Running in debug mode (slow motion)" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "🚀 Starting Playwright tests..." -ForegroundColor Green
Write-Host ""

try {
    Push-Location $ProjectRoot
    & dotnet @TestArgs
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ All tests passed!" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "❌ Some tests failed. Check the output above for details." -ForegroundColor Red
        exit $LASTEXITCODE
    }
} catch {
    Write-Host "❌ Error running tests: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}

Write-Host ""
Write-Host "📋 Test artifacts saved to:" -ForegroundColor Cyan
Write-Host "   Screenshots: tests/TagManagement.PlaywrightTests/test-results/" -ForegroundColor White
Write-Host "   Videos: tests/TagManagement.PlaywrightTests/test-results/" -ForegroundColor White
Write-Host ""
Write-Host "💡 Tips:" -ForegroundColor Cyan
Write-Host "   Use -Headed to see browser UI" -ForegroundColor White
Write-Host "   Use -Debug for slow motion debugging" -ForegroundColor White
Write-Host "   Use -Test 'pattern' to run specific tests" -ForegroundColor White
