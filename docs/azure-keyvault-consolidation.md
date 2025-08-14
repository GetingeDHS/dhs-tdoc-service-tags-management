# Azure Key Vault Consolidation Strategy

## Overview

This document outlines the strategy for consolidating Key Vaults across development and test environments for the DHS TDOC Tag Management service to reduce costs and complexity while maintaining security and compliance.

## Current State vs. Target State

### Current State (Before Consolidation)
- **Individual Key Vaults per environment**: Each deployment creates its own Key Vault
  - `tagmgmt-dev-kv-{suffix}` for development
  - Ephemeral test Key Vaults created/destroyed for each E2E test run
  - `tagmgmt-prod-kv-{suffix}` for production (separate and secure)

### Target State (After Consolidation)
- **Shared Key Vault for non-production**: Single Key Vault for dev/test environments
  - `kv-dhs-tdoc-shared-{suffix}` for dev and test environments
  - `tagmgmt-prod-kv-{suffix}` remains separate for production (compliance requirement)
  - Secrets organized by environment prefix: `sql-connection-string-dev`, `sql-connection-string-test`

## Benefits

### 💰 Cost Reduction
- **Estimated savings**: $15-30/month per eliminated Key Vault
- **Base cost elimination**: Key Vault has a base cost regardless of usage
- **Operations cost**: Reduced monitoring and maintenance overhead

### 🔧 Operational Efficiency  
- **Centralized secret management**: All non-prod secrets in one location
- **Simplified rotation**: Update secrets once, available to all non-prod environments
- **Easier troubleshooting**: Single location to check for secret-related issues
- **Reduced cognitive load**: Fewer resources to track and manage

### 🛡️ Security Benefits
- **Consistent access policies**: Standardized permissions across environments
- **Centralized audit trail**: All non-prod secret access logged in one place
- **Reduced attack surface**: Fewer Key Vaults to secure and monitor

## Implementation Strategy

### Phase 1: Create Shared Key Vault Infrastructure

1. **Deploy shared Key Vault** using the new `shared-keyvault.tf` configuration
2. **Configure access policies** for service principals and applications
3. **Migrate existing secrets** from individual Key Vaults to shared one

### Phase 2: Update Applications and Pipelines

1. **Update Terraform configurations** to reference shared Key Vault
2. **Modify CI/CD pipelines** to use shared Key Vault for non-prod deployments
3. **Update application configurations** to point to shared Key Vault

### Phase 3: Cleanup and Validation

1. **Remove old Key Vault configurations** from Terraform
2. **Run cleanup scripts** to remove orphaned Key Vaults
3. **Validate all environments** are working with shared Key Vault
4. **Monitor cost savings** and operational improvements

## Security Considerations

### Access Control
```hcl
# Environment-specific access policies
resource "azurerm_key_vault_access_policy" "dev_team" {
  key_vault_id = azurerm_key_vault.shared.id
  # ... dev team permissions
}

resource "azurerm_key_vault_access_policy" "test_automation" {
  key_vault_id = azurerm_key_vault.shared.id  
  # ... test automation permissions
}
```

### Secret Naming Convention
```
# Environment prefixed secrets
sql-connection-string-dev
sql-connection-string-test
app-insights-key-dev
app-insights-key-test
certificate-dev
certificate-test
```

### Network Security
- **Allow Azure Services**: Enables App Services and Container Apps to access
- **Restrict public access**: Consider private endpoints for production
- **Audit logging**: All access logged to Log Analytics workspace

## Medical Device Compliance

### Separation of Concerns
- **Production isolation**: Production Key Vault remains completely separate
- **Development data**: No production data or secrets in shared Key Vault
- **Audit trail**: Comprehensive logging maintained for compliance

### Access Controls
- **Principle of least privilege**: Role-based access with minimal required permissions
- **Regular access reviews**: Quarterly review of access policies
- **Emergency access**: Break-glass procedures documented

## Migration Plan

### Pre-Migration Checklist
- [ ] Backup existing Key Vault contents
- [ ] Document current secret usage
- [ ] Test shared Key Vault in isolated environment
- [ ] Update all Terraform configurations
- [ ] Prepare rollback plan

### Migration Steps

1. **Create shared Key Vault**
   ```bash
   cd infrastructure/terraform
   terraform apply -var="environment=dev"
   ```

2. **Migrate secrets**
   ```bash
   # Copy secrets from old to new Key Vault
   ./scripts/migrate-keyvault-secrets.ps1 -SourceVault "old-kv" -TargetVault "shared-kv"
   ```

3. **Update applications**
   ```bash
   # Update connection strings and Key Vault references
   # Deploy updated applications
   ```

4. **Validate functionality**
   ```bash
   # Run integration tests
   # Verify secret access
   # Check audit logs
   ```

5. **Cleanup old resources**
   ```bash
   ./scripts/cleanup-azure-resources.ps1 -Environment dev -DryRun
   ```

### Post-Migration Validation

- [ ] All applications can access required secrets
- [ ] Audit logs show successful secret retrievals
- [ ] No references to old Key Vaults remain
- [ ] Cost monitoring shows expected savings
- [ ] Security scanning passes with new configuration

## Monitoring and Alerting

### Key Metrics to Monitor
- **Secret access frequency**: Unusual patterns may indicate issues
- **Failed access attempts**: Security concern requiring investigation  
- **Key Vault availability**: Ensure high availability for shared resource
- **Cost tracking**: Validate expected cost savings

### Alerts Configuration
```hcl
# Alert on failed Key Vault access
resource "azurerm_monitor_metric_alert" "keyvault_failures" {
  name = "shared-keyvault-access-failures"
  # ... alert configuration
  criteria {
    metric_name = "ServiceApiResult"
    operator    = "GreaterThan"
    threshold   = 5
  }
}
```

## Rollback Plan

### Emergency Rollback
1. **Redeploy individual Key Vaults** using previous Terraform configuration
2. **Restore secrets** from backups to individual Key Vaults
3. **Update applications** to use individual Key Vaults
4. **Monitor and validate** all services are operational

### Gradual Rollback
1. **Create new individual Key Vaults** alongside shared one
2. **Migrate secrets back** environment by environment
3. **Update applications** one at a time
4. **Retire shared Key Vault** once all services migrated back

## Cost Analysis

### Current Costs (Estimated)
- Development Key Vault: $15/month
- Test Key Vaults (ephemeral): $10/month average
- Operations overhead: $20/month equivalent
- **Total**: ~$45/month

### Target Costs (Estimated)  
- Shared Key Vault: $15/month
- Operations overhead: $10/month equivalent
- **Total**: ~$25/month

### **Net Savings**: $20/month (~44% reduction)

## Next Steps

1. **Review and approve** this consolidation strategy
2. **Schedule implementation** during low-traffic period
3. **Execute Phase 1** - Deploy shared Key Vault infrastructure
4. **Test thoroughly** in development environment
5. **Roll out to test environment** with monitoring
6. **Document lessons learned** and update procedures

---

## Resources

- [Azure Key Vault Best Practices](https://docs.microsoft.com/en-us/azure/key-vault/general/best-practices)
- [Key Vault Security Overview](https://docs.microsoft.com/en-us/azure/key-vault/general/security-overview)
- [Medical Device Compliance with Azure](https://docs.microsoft.com/en-us/azure/compliance/)
- [Terraform Azure Key Vault Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/key_vault)
