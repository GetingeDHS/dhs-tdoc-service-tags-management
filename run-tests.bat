@echo off
setlocal enabledelayedexpansion

echo ====================================================================
echo Medical Device Tag Management Service - Test Execution
echo Compliance Standard: ISO-13485
echo ====================================================================

REM Set variables
set SOLUTION_PATH=TagManagement.sln
set OUTPUT_DIR=TestResults
set COVERAGE_THRESHOLD=95
set PROJECT_NAME=Tag Management Service

echo [INFO] Cleaning previous test results...
if exist %OUTPUT_DIR% rmdir /s /q %OUTPUT_DIR%
mkdir %OUTPUT_DIR%

echo [INFO] Building solution...
dotnet build %SOLUTION_PATH% --configuration Release --verbosity minimal
if %ERRORLEVEL% neq 0 (
    echo [ERROR] Build failed!
    exit /b 1
)

echo [INFO] Running unit tests with coverage analysis...
dotnet test %SOLUTION_PATH% ^
    --configuration Release ^
    --no-build ^
    --logger "trx;LogFileName=test-results.trx" ^
    --logger "console;verbosity=normal" ^
    --collect:"XPlat Code Coverage" ^
    --results-directory "%OUTPUT_DIR%" ^
    --settings coverlet.runsettings ^
    /p:CoverletOutputFormat=cobertura ^
    /p:CoverletOutput="%OUTPUT_DIR%/" ^
    /p:Threshold=%COVERAGE_THRESHOLD% ^
    /p:ThresholdType=line,branch,method ^
    /p:ThresholdStat=minimum

set TEST_EXIT_CODE=%ERRORLEVEL%

echo [INFO] Generating comprehensive test reports...

REM Check if test report generator exists
if exist "tools\TestReporting\bin\Release\net8.0\TestReportGenerator.exe" (
    echo [INFO] Using compiled test report generator...
    "tools\TestReporting\bin\Release\net8.0\TestReportGenerator.exe" generate ^
        --test-results "%OUTPUT_DIR%" ^
        --coverage-report "%OUTPUT_DIR%\coverage.cobertura.xml" ^
        --output-dir "%OUTPUT_DIR%" ^
        --project-name "%PROJECT_NAME%" ^
        --compliance-standard "ISO-13485" ^
        --include-medical-validation true
) else (
    echo [INFO] Building and running test report generator...
    dotnet build tools\TestReporting\TestReportGenerator.csproj --configuration Release --verbosity minimal
    if !ERRORLEVEL! equ 0 (
        dotnet run --project tools\TestReporting\TestReportGenerator.csproj --configuration Release -- generate ^
            --test-results "%OUTPUT_DIR%" ^
            --coverage-report "%OUTPUT_DIR%\coverage.cobertura.xml" ^
            --output-dir "%OUTPUT_DIR%" ^
            --project-name "%PROJECT_NAME%" ^
            --compliance-standard "ISO-13485" ^
            --include-medical-validation true
    ) else (
        echo [WARNING] Failed to build test report generator, generating basic summary...
        echo Test execution completed with exit code: %TEST_EXIT_CODE% > "%OUTPUT_DIR%\TestSummary.txt"
        echo Detailed coverage and compliance reports are not available. >> "%OUTPUT_DIR%\TestSummary.txt"
    )
)

echo [INFO] Generating HTML coverage report using ReportGenerator...
dotnet tool list -g | findstr "dotnet-reportgenerator-globaltool" >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo [INFO] Installing ReportGenerator global tool...
    dotnet tool install -g dotnet-reportgenerator-globaltool
)

REM Find coverage files
for /f "delims=" %%i in ('dir "%OUTPUT_DIR%\*\coverage.cobertura.xml" /s /b 2^>nul') do (
    set COVERAGE_FILE=%%i
    goto :found_coverage
)

REM Try alternative location
if exist "%OUTPUT_DIR%\coverage.cobertura.xml" (
    set COVERAGE_FILE=%OUTPUT_DIR%\coverage.cobertura.xml
    goto :found_coverage
)

echo [WARNING] Coverage file not found, skipping HTML report generation
goto :skip_html_report

:found_coverage
echo [INFO] Generating HTML coverage report from: !COVERAGE_FILE!
reportgenerator ^
    "-reports:!COVERAGE_FILE!" ^
    "-targetdir:%OUTPUT_DIR%\CoverageReport" ^
    "-reporttypes:Html;HtmlSummary;Badges;TextSummary" ^
    "-title:Tag Management Service Coverage Report" ^
    "-tag:ISO-13485;Medical Device" ^
    "-historydir:%OUTPUT_DIR%\CoverageHistory"

:skip_html_report

echo [INFO] Test execution completed!
echo ====================================================================
echo Results Summary:
echo - Test exit code: %TEST_EXIT_CODE%
echo - Results directory: %OUTPUT_DIR%
echo - Main report: %OUTPUT_DIR%\TestReport.html
echo - Coverage report: %OUTPUT_DIR%\CoverageReport\index.html
echo - Medical validation: %OUTPUT_DIR%\MedicalDeviceValidation.html
echo - Compliance summary: %OUTPUT_DIR%\ComplianceSummary.txt
echo ====================================================================

if %TEST_EXIT_CODE% neq 0 (
    echo [ERROR] Tests failed or coverage threshold not met!
    echo Please review the test results and fix any issues.
    exit /b %TEST_EXIT_CODE%
) else (
    echo [SUCCESS] All tests passed and coverage requirements met!
)

REM Open main report if available
if exist "%OUTPUT_DIR%\TestReport.html" (
    echo [INFO] Opening test report in browser...
    start "" "%OUTPUT_DIR%\TestReport.html"
)

exit /b %TEST_EXIT_CODE%
