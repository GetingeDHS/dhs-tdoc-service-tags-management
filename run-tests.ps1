# Medical Device Tag Management Service - Test Execution Script
# Compliance Standard: ISO-13485
param(
    [string]$SolutionPath = "TagManagement.sln",
    [string]$OutputDir = "TestResults",
    [int]$CoverageThreshold = 95,
    [string]$ProjectName = "Tag Management Service",
    [string]$Configuration = "Release",
    [switch]$SkipBuild = $false,
    [switch]$OpenReports = $true,
    [switch]$InstallTools = $true
)

Write-Host "====================================================================" -ForegroundColor Cyan
Write-Host "Medical Device Tag Management Service - Test Execution" -ForegroundColor Cyan
Write-Host "Compliance Standard: ISO-13485" -ForegroundColor Cyan
Write-Host "====================================================================" -ForegroundColor Cyan

# Function to write colored output
function Write-Status {
    param([string]$Message, [string]$Type = "INFO")
    
    switch ($Type) {
        "INFO" { Write-Host "[$Type] $Message" -ForegroundColor Green }
        "WARNING" { Write-Host "[$Type] $Message" -ForegroundColor Yellow }
        "ERROR" { Write-Host "[$Type] $Message" -ForegroundColor Red }
        "SUCCESS" { Write-Host "[$Type] $Message" -ForegroundColor Cyan }
        default { Write-Host "[$Type] $Message" }
    }
}

# Function to check if a command exists
function Test-CommandExists {
    param([string]$Command)
    
    try {
        Get-Command $Command -ErrorAction Stop | Out-Null
        return $true
    }
    catch {
        return $false
    }
}

# Clean previous test results
Write-Status "Cleaning previous test results..."
if (Test-Path $OutputDir) {
    Remove-Item -Path $OutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

# Build solution
if (-not $SkipBuild) {
    Write-Status "Building solution..."
    $buildResult = dotnet build $SolutionPath --configuration $Configuration --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        Write-Status "Build failed!" -Type "ERROR"
        exit 1
    }
}

# Run tests with coverage
Write-Status "Running unit tests with coverage analysis..."
$testCommand = @(
    "test", $SolutionPath,
    "--configuration", $Configuration,
    "--logger", "trx;LogFileName=test-results.trx",
    "--logger", "console;verbosity=normal",
    "--collect:XPlat Code Coverage",
    "--results-directory", $OutputDir,
    "--settings", "coverlet.runsettings",
    "/p:CoverletOutputFormat=cobertura",
    "/p:CoverletOutput=$OutputDir/",
    "/p:Threshold=$CoverageThreshold",
    "/p:ThresholdType=line,branch,method",
    "/p:ThresholdStat=minimum"
)

if (-not $SkipBuild) {
    $testCommand += "--no-build"
}

& dotnet @testCommand
$testExitCode = $LASTEXITCODE

Write-Status "Test execution completed with exit code: $testExitCode"

# Generate comprehensive reports
Write-Status "Generating comprehensive test reports..."

# Check for compiled test report generator
$testReportExe = "tools\TestReporting\bin\$Configuration\net8.0\TestReportGenerator.exe"
if (Test-Path $testReportExe) {
    Write-Status "Using compiled test report generator..."
    & $testReportExe generate `
        --test-results $OutputDir `
        --coverage-report "$OutputDir\coverage.cobertura.xml" `
        --output-dir $OutputDir `
        --project-name $ProjectName `
        --compliance-standard "ISO-13485" `
        --include-medical-validation true
} else {
    # Build and run test report generator
    Write-Status "Building and running test report generator..."
    $buildReportResult = dotnet build "tools\TestReporting\TestReportGenerator.csproj" --configuration $Configuration --verbosity minimal
    
    if ($LASTEXITCODE -eq 0) {
        & dotnet run --project "tools\TestReporting\TestReportGenerator.csproj" --configuration $Configuration -- generate `
            --test-results $OutputDir `
            --coverage-report "$OutputDir\coverage.cobertura.xml" `
            --output-dir $OutputDir `
            --project-name $ProjectName `
            --compliance-standard "ISO-13485" `
            --include-medical-validation true
    } else {
        Write-Status "Failed to build test report generator, generating basic summary..." -Type "WARNING"
        "Test execution completed with exit code: $testExitCode" | Out-File -FilePath "$OutputDir\TestSummary.txt"
        "Detailed coverage and compliance reports are not available." | Add-Content -Path "$OutputDir\TestSummary.txt"
    }
}

# Install and use ReportGenerator for HTML coverage reports
if ($InstallTools) {
    Write-Status "Checking ReportGenerator global tool..."
    $hasReportGenerator = dotnet tool list -g | Select-String "dotnet-reportgenerator-globaltool"
    
    if (-not $hasReportGenerator) {
        Write-Status "Installing ReportGenerator global tool..."
        dotnet tool install -g dotnet-reportgenerator-globaltool
    }
}

# Find coverage file
$coverageFile = $null
$possiblePaths = @(
    "$OutputDir\coverage.cobertura.xml",
    "$OutputDir\*\coverage.cobertura.xml"
)

foreach ($path in $possiblePaths) {
    $files = Get-ChildItem -Path $path -ErrorAction SilentlyContinue
    if ($files) {
        $coverageFile = $files[0].FullName
        break
    }
}

if ($coverageFile) {
    Write-Status "Generating HTML coverage report from: $coverageFile"
    
    if (Test-CommandExists "reportgenerator") {
        reportgenerator `
            "-reports:$coverageFile" `
            "-targetdir:$OutputDir\CoverageReport" `
            "-reporttypes:Html;HtmlSummary;Badges;TextSummary" `
            "-title:Tag Management Service Coverage Report" `
            "-tag:ISO-13485;Medical Device" `
            "-historydir:$OutputDir\CoverageHistory"
    } else {
        Write-Status "ReportGenerator not available, skipping HTML coverage report" -Type "WARNING"
    }
} else {
    Write-Status "Coverage file not found, skipping HTML report generation" -Type "WARNING"
}

# Generate summary
Write-Status "Test execution completed!" -Type "SUCCESS"
Write-Host "====================================================================" -ForegroundColor Cyan
Write-Host "Results Summary:" -ForegroundColor White
Write-Host "- Test exit code: $testExitCode" -ForegroundColor White
Write-Host "- Results directory: $OutputDir" -ForegroundColor White
Write-Host "- Main report: $OutputDir\TestReport.html" -ForegroundColor White
Write-Host "- Coverage report: $OutputDir\CoverageReport\index.html" -ForegroundColor White
Write-Host "- Medical validation: $OutputDir\MedicalDeviceValidation.html" -ForegroundColor White
Write-Host "- Compliance summary: $OutputDir\ComplianceSummary.txt" -ForegroundColor White
Write-Host "====================================================================" -ForegroundColor Cyan

# Evaluate results
if ($testExitCode -ne 0) {
    Write-Status "Tests failed or coverage threshold not met!" -Type "ERROR"
    Write-Status "Please review the test results and fix any issues." -Type "ERROR"
} else {
    Write-Status "All tests passed and coverage requirements met!" -Type "SUCCESS"
}

# Open reports
if ($OpenReports -and (Test-Path "$OutputDir\TestReport.html")) {
    Write-Status "Opening test report in browser..."
    Start-Process "$OutputDir\TestReport.html"
}

# Generate CI/CD friendly output
if ($env:CI -or $env:TF_BUILD -or $env:GITHUB_ACTIONS) {
    Write-Host "##vso[task.uploadfile]$OutputDir\TestReport.html"
    Write-Host "##vso[task.uploadfile]$OutputDir\ComplianceSummary.txt"
    
    if (Test-Path "$OutputDir\CoverageReport\index.html") {
        Write-Host "##vso[task.uploadfile]$OutputDir\CoverageReport\index.html"
    }
}

exit $testExitCode
