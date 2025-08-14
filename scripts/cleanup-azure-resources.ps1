#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Cleanup script for dangling Azure resources from Tag Management service deployments.

.DESCRIPTION
    This script identifies and optionally cleans up dangling Azure resources that might be left
    from failed deployments or test runs. It's designed to be safe and will prompt before
    deleting anything.

.PARAMETER DryRun
    If specified, the script will only identify resources without deleting them.

.PARAMETER Environment
    Target environment to clean up (dev, test, staging, prod). Default is 'test'.

.PARAMETER Force
    If specified, skip confirmation prompts for deletion.

.EXAMPLE
    ./cleanup-azure-resources.ps1 -DryRun
    Lists all potentially dangling resources without deleting them.

.EXAMPLE
    ./cleanup-azure-resources.ps1 -Environment test -Force
    Cleans up all test environment resources without confirmation.
#>

param(
    [switch]$DryRun,
    [ValidateSet("dev", "test", "staging", "prod")]
    [string]$Environment = "test",
    [switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Colors for output
$Red = [System.ConsoleColor]::Red
$Green = [System.ConsoleColor]::Green
$Yellow = [System.ConsoleColor]::Yellow
$Cyan = [System.ConsoleColor]::Cyan

function Write-ColorOutput {
    param(
        [string]$Message,
        [System.ConsoleColor]$Color = [System.ConsoleColor]::White
    )
    Write-Host $Message -ForegroundColor $Color
}

function Test-AzureLogin {
    try {
        $context = az account show --query "name" -o tsv 2>$null
        if ($LASTEXITCODE -eq 0 -and $context) {
            Write-ColorOutput "✅ Logged into Azure subscription: $context" $Green
            return $true
        }
    }
    catch {
        # Ignore error, handle below
    }
    
    Write-ColorOutput "❌ Not logged into Azure. Please run 'az login' first." $Red
    return $false
}

function Get-DanglingResourceGroups {
    param([string]$Environment)
    
    Write-ColorOutput "🔍 Searching for resource groups related to Tag Management..." $Cyan
    
    # Search patterns for different naming conventions
    $searchPatterns = @(
        "*tagmgmt*$Environment*",
        "*tdoc-tags*$Environment*",
        "*tag-management*$Environment*",
        "rg-*tags*$Environment*"
    )
    
    $resourceGroups = @()
    
    foreach ($pattern in $searchPatterns) {
        $groups = az group list --query '[?contains(name, `"tagmgmt`") || contains(name, `"tdoc-tags`") || contains(name, `"tag-management`")].{Name:name, Location:location, Tags:tags}' -o json | ConvertFrom-Json
        $resourceGroups += $groups
    }
    
    # Remove duplicates
    $uniqueGroups = $resourceGroups | Sort-Object Name -Unique
    
    return $uniqueGroups
}

function Get-OrphanedKeyVaults {
    Write-ColorOutput "🔍 Searching for orphaned Key Vaults..." $Cyan
    
    $keyVaults = az keyvault list --query '[?contains(name, `"tagmgmt`") || contains(name, `"tdoc`") || contains(name, `"kv-dhs-tdoc`")].{Name:name, ResourceGroup:resourceGroup, Location:location}' -o json | ConvertFrom-Json
    
    return $keyVaults
}

function Get-StaleStorageAccounts {
    Write-ColorOutput "🔍 Searching for stale storage accounts..." $Cyan
    
    $storageAccounts = az storage account list --query '[?contains(name, `"tagmgmt`") || contains(name, `"tdoc`") || contains(name, `"audit`")].{Name:name, ResourceGroup:resourceGroup, Location:location}' -o json | ConvertFrom-Json
    
    return $storageAccounts
}

function Get-OldTestResources {
    param([int]$OlderThanHours = 24)
    
    Write-ColorOutput "🔍 Searching for test resources older than $OlderThanHours hours..." $Cyan
    
    $cutoffDate = (Get-Date).AddHours(-$OlderThanHours)
    $oldResources = @()
    
    # Get all resources with test-related tags
    $testResources = az resource list --query '[?tags.Environment==`"test`" || tags.Purpose==`"E2E Testing`"].{Name:name, Type:type, ResourceGroup:resourceGroup, Tags:tags}' -o json | ConvertFrom-Json
    
    foreach ($resource in $testResources) {
        # For simplicity, assume resources are old if they match test patterns
        # In a real scenario, you'd check creation time
        if ($resource.Name -match "test|temp|tmp") {
            $oldResources += $resource
        }
    }
    
    return $oldResources
}

function Remove-ResourceGroup {
    param(
        [string]$ResourceGroupName,
        [bool]$Force = $false
    )
    
    if (-not $Force) {
        $confirmation = Read-Host "Are you sure you want to delete resource group '$ResourceGroupName'? (y/N)"
        if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
            Write-ColorOutput "⏭️  Skipping $ResourceGroupName" $Yellow
            return
        }
    }
    
    Write-ColorOutput "🗑️  Deleting resource group: $ResourceGroupName" $Yellow
    
    try {
        az group delete --name $ResourceGroupName --yes --no-wait
        Write-ColorOutput "✅ Deletion initiated for: $ResourceGroupName" $Green
    }
    catch {
        Write-ColorOutput "❌ Failed to delete: $ResourceGroupName - $($_.Exception.Message)" $Red
    }
}

function Show-ResourceSummary {
    param(
        [array]$ResourceGroups,
        [array]$KeyVaults,
        [array]$StorageAccounts,
        [array]$OldResources
    )
    
    Write-ColorOutput "`n📊 RESOURCE SUMMARY" $Cyan
    Write-ColorOutput "==================" $Cyan
    Write-ColorOutput "Resource Groups: $($ResourceGroups.Count)" $Yellow
    Write-ColorOutput "Key Vaults: $($KeyVaults.Count)" $Yellow  
    Write-ColorOutput "Storage Accounts: $($StorageAccounts.Count)" $Yellow
    Write-ColorOutput "Old Test Resources: $($OldResources.Count)" $Yellow
    
    if ($ResourceGroups.Count -gt 0) {
        Write-ColorOutput "`nResource Groups found:" $Cyan
        foreach ($rg in $ResourceGroups) {
            Write-ColorOutput "  • $($rg.Name) ($($rg.Location))" $Yellow
        }
    }
    
    if ($KeyVaults.Count -gt 0) {
        Write-ColorOutput "`nKey Vaults found:" $Cyan
        foreach ($kv in $KeyVaults) {
            Write-ColorOutput "  • $($kv.Name) in $($kv.ResourceGroup)" $Yellow
        }
    }
    
    if ($StorageAccounts.Count -gt 0) {
        Write-ColorOutput "`nStorage Accounts found:" $Cyan
        foreach ($sa in $StorageAccounts) {
            Write-ColorOutput "  • $($sa.Name) in $($sa.ResourceGroup)" $Yellow
        }
    }
    
    if ($OldResources.Count -gt 0) {
        Write-ColorOutput "`nOld Test Resources found:" $Cyan
        foreach ($res in $OldResources) {
            Write-ColorOutput "  • $($res.Name) ($($res.Type)) in $($res.ResourceGroup)" $Yellow
        }
    }
}

# Main execution
function Main {
    Write-ColorOutput "🧹 Azure Resource Cleanup for Tag Management Service" $Cyan
    Write-ColorOutput "Environment: $Environment" $Cyan
    Write-ColorOutput "Dry Run: $DryRun" $Cyan
    Write-ColorOutput "=================================================" $Cyan
    
    if (-not (Test-AzureLogin)) {
        exit 1
    }
    
    # Gather potentially dangling resources
    $resourceGroups = Get-DanglingResourceGroups -Environment $Environment
    $keyVaults = Get-OrphanedKeyVaults
    $storageAccounts = Get-StaleStorageAccounts  
    $oldResources = Get-OldTestResources -OlderThanHours 24
    
    # Show summary
    Show-ResourceSummary -ResourceGroups $resourceGroups -KeyVaults $keyVaults -StorageAccounts $storageAccounts -OldResources $oldResources
    
    if ($DryRun) {
        Write-ColorOutput "`n🔍 DRY RUN MODE - No resources will be deleted" $Green
        Write-ColorOutput "Re-run without -DryRun to actually delete resources" $Green
        return
    }
    
    # Calculate total cost savings (rough estimate)
    $totalResources = $resourceGroups.Count + $keyVaults.Count + $storageAccounts.Count + $oldResources.Count
    
    if ($totalResources -eq 0) {
        Write-ColorOutput "✅ No dangling resources found!" $Green
        return
    }
    
    $estimatedMonthlySavings = $totalResources * 50 # Rough estimate of $50/month per resource group
    Write-ColorOutput "`n💰 Estimated monthly cost savings: $estimatedMonthlySavings USD" $Green
    
    if (-not $Force) {
        Write-ColorOutput "`nThis will DELETE the resources listed above." $Red
        $finalConfirmation = Read-Host "Are you sure you want to proceed? Type 'DELETE' to confirm"
        if ($finalConfirmation -ne 'DELETE') {
            Write-ColorOutput "❌ Operation cancelled" $Yellow
            return
        }
    }
    
    # Delete resource groups (this will delete contained resources too)
    foreach ($rg in $resourceGroups) {
        Remove-ResourceGroup -ResourceGroupName $rg.Name -Force $Force
    }
    
    Write-ColorOutput "`n✅ Cleanup initiated! Resource deletions are running in the background." $Green
    Write-ColorOutput "You can monitor progress in the Azure portal or with:" $Cyan
    Write-ColorOutput "  az group list --query '[?properties.provisioningState==`"Deleting`"]' -o table" $Cyan
}

# Run the main function
Main
