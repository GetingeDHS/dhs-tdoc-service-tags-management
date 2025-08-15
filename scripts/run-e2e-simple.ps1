param(
    [string]$Test = "",
    [switch]$Headed = $false,
    [switch]$Debug = $false
)

$ProjectRoot = Split-Path -Parent $PSScriptRoot
$PlaywrightProject = Join-Path (Join-Path $ProjectRoot "tests") "TagManagement.PlaywrightTests"

Write-Host "Running Local E2E Tests" -ForegroundColor Cyan
Write-Host "=======================" -ForegroundColor Cyan

# Check if API is running locally
Write-Host "Checking if local API is running..." -ForegroundColor Yellow

try {
    $Response = Invoke-RestMethod -Uri "http://localhost:5196/health" -Method Get -TimeoutSec 5
    if ($Response.status -eq "Healthy") {
        Write-Host "API is healthy and ready" -ForegroundColor Green
    } else {
        Write-Host "API responded but status is: $($Response.status)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "Cannot reach local API at http://localhost:5196" -ForegroundColor Red
    Write-Host "Please start the API first using:" -ForegroundColor Red
    Write-Host "dotnet run --project src/TagManagement.Api" -ForegroundColor Red
    exit 1
}

# Build test arguments
$TestArgs = @(
    "test"
    $PlaywrightProject
    "--configuration", "Debug"
    "--logger", "console;verbosity=normal"
)

if ($Test) {
    $TestArgs += "--filter", "Name~$Test"
    Write-Host "Running specific test pattern: $Test" -ForegroundColor Cyan
} else {
    Write-Host "Running all E2E tests" -ForegroundColor Cyan
}

Write-Host "Starting Playwright tests..." -ForegroundColor Green

try {
    Push-Location $ProjectRoot
    & dotnet @TestArgs
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "All tests passed!" -ForegroundColor Green
    } else {
        Write-Host "Some tests failed. Check the output above for details." -ForegroundColor Red
        exit $LASTEXITCODE
    }
} catch {
    Write-Host "Error running tests: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}

Write-Host "Test artifacts saved to: tests/TagManagement.PlaywrightTests/test-results/" -ForegroundColor Cyan
