Write-Host "Testing PowerShell script execution" -ForegroundColor Green

$testVar = $true

if ($testVar) {
    Write-Host "✅ If statement works" -ForegroundColor Green
} else {
    Write-Host "❌ If statement failed" -ForegroundColor Red
}

Write-Host "Script completed successfully" -ForegroundColor Cyan
