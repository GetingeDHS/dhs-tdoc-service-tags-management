# Tag Management Service - Shared Infrastructure Integration

## Overview

The DHS TDOC Tag Management service has been fully integrated with the DHS-AIRE shared infrastructure to optimize costs, improve operational efficiency, and maintain naming consistency across all projects.

## Shared Infrastructure Components

### 🔗 What We Use from Shared Infrastructure

1. **Azure Container Registry (ACR)**
   - **Name**: `dhsairecr`
   - **Resource Group**: `dhs-aire-shared-rg`
   - **Usage**: Container image storage for `dhs-aire-tagmanagement:latest`

2. **Shared Key Vault** (non-production only)
   - **Name**: `dhs-aire-shared-kv`
   - **Resource Group**: `dhs-aire-shared-rg`
   - **Purpose**: Centralized secret management for dev/test environments

### 🏗️ Infrastructure Architecture

```
DHS-AIRE Shared Infrastructure
├── dhs-aire-shared-rg/
│   ├── dhsairecr (Container Registry)
│   └── dhs-aire-shared-kv (Key Vault)
│
Tag Management Service (Per Environment)
├── dhs-aire-tagmgmt-{env}-rg-{suffix}/
│   ├── SQL Server & Database
│   ├── Container App Environment
│   ├── Container App
│   ├── Application Insights
│   ├── Log Analytics Workspace
│   ├── Storage Account (audit logs)
│   ├── Key Vault (production only)
│   └── Monitoring & Alerts
```

## Naming Standards Alignment

### Before Integration
- Resource prefix: `tagmgmt-{environment}`
- Container images: Individual ACR per environment
- Key Vaults: Individual per environment

### After Integration
- Resource prefix: `dhs-aire-tagmgmt-{environment}`
- Container images: `dhsairecr.azurecr.io/dhs-aire-tagmanagement:latest`
- Key Vaults: Shared for non-prod, dedicated for production
- Secret naming: `tagmgmt-{secret-name}-{environment}`

## Key Vault Strategy

### Non-Production Environments (dev, test, staging)
- **Location**: Shared Key Vault (`dhs-aire-shared-kv`)
- **Secrets**: Environment-prefixed for isolation
- **Cost Savings**: ~$15-30/month per eliminated Key Vault

### Production Environment
- **Location**: Dedicated Key Vault (`dhs-aire-tagmgmt-prod-kv-{suffix}`)
- **SKU**: Premium (enhanced security features)
- **Compliance**: Full isolation for production secrets

### Secret Management Examples

```hcl
# Non-production secrets in shared Key Vault
resource "azurerm_key_vault_secret" "database_connection_string" {
  name         = "tagmgmt-sql-connection-string-${var.environment}"
  value        = var.connection_string
  key_vault_id = data.azurerm_key_vault.shared.id
}

# Production secrets in dedicated Key Vault
resource "azurerm_key_vault_secret" "database_connection_string_prod" {
  name         = "tagmgmt-sql-connection-string-prod"
  value        = var.connection_string
  key_vault_id = azurerm_key_vault.main.id
}
```

## Container Registry Integration

### Image Naming Convention
```
dhsairecr.azurecr.io/dhs-aire-tagmanagement:latest
dhsairecr.azurecr.io/dhs-aire-tagmanagement:v1.0.0
dhsairecr.azurecr.io/dhs-aire-tagmanagement:dev-20231214
```

### Usage in Terraform
```hcl
# Reference shared container registry
data "azurerm_container_registry" "shared" {
  name                = "dhsairecr"
  resource_group_name = "dhs-aire-shared-rg"
}

# Use in container app
resource "azurerm_container_app" "tagmanagement" {
  template {
    container {
      image = "${data.azurerm_container_registry.shared.login_server}/dhs-aire-tagmanagement:latest"
    }
  }
}
```

## Access Control & Security

### Key Vault Access Policies

#### Shared Key Vault (Non-Production)
```hcl
# Container App access
resource "azurerm_key_vault_access_policy" "container_app_shared" {
  key_vault_id       = data.azurerm_key_vault.shared.id
  object_id          = azurerm_container_app.tagmanagement.identity[0].principal_id
  secret_permissions = ["Get", "List"]
}

# Terraform access for secret management
resource "azurerm_key_vault_access_policy" "terraform_shared" {
  key_vault_id       = data.azurerm_key_vault.shared.id
  object_id          = data.azurerm_client_config.current.object_id
  secret_permissions = ["Get", "List", "Set", "Delete", "Purge", "Recover"]
}
```

#### Dedicated Key Vault (Production)
```hcl
# Production Key Vault with enhanced security
resource "azurerm_key_vault" "main" {
  sku_name                   = "premium"
  soft_delete_retention_days = 90
  purge_protection_enabled   = true
}
```

## Cost Optimization Benefits

### Container Registry Consolidation
- **Before**: Individual ACR per project (~$5/month each)
- **After**: Shared ACR across all DHS-AIRE projects
- **Savings**: $5/month per project + operational overhead reduction

### Key Vault Consolidation
- **Before**: Individual Key Vault per environment
- **After**: Shared Key Vault for non-production
- **Savings**: $15-30/month per eliminated Key Vault
- **Total estimated savings**: $20-40/month for Tag Management service

## Implementation Status

### ✅ Completed
- [x] Shared Container Registry integration
- [x] Shared Key Vault configuration
- [x] Naming standards alignment
- [x] Terraform configuration updates
- [x] Test environment integration
- [x] Access policies configuration
- [x] Secret management setup

### 📋 Next Steps
1. **Deploy shared Key Vault** (if not already deployed)
2. **Test integration** in development environment
3. **Validate secret access** from container apps
4. **Monitor cost savings** and operational improvements

## Deployment Prerequisites

### Shared Infrastructure Must Be Deployed First
```bash
# Deploy shared infrastructure
cd ../dhs-aire-infrastructure/terraform
terraform apply -var="create_shared_keyvault=true"
```

### Then Deploy Tag Management Service
```bash
# Deploy tag management service
cd infrastructure/terraform
terraform apply -var="environment=dev"
```

## Configuration Files Updated

### Tag Management Service
- `infrastructure/terraform/main.tf` - Main infrastructure configuration
- `infrastructure/terraform/variables.tf` - Variable definitions
- `infrastructure/azure/test-environment/main.tf` - Test environment configuration

### Shared Infrastructure
- `terraform/main.tf` - Added Key Vault resource and access policies
- `terraform/variables.tf` - Added Key Vault configuration variables
- `terraform/outputs.tf` - Added Key Vault outputs
- `README.md` - Updated documentation with Key Vault usage

## Monitoring & Troubleshooting

### Verify Shared Infrastructure
```bash
# Check shared resources
az resource list --resource-group dhs-aire-shared-rg --output table

# Verify Key Vault access
az keyvault list --query "[?name=='dhs-aire-shared-kv']" --output table
```

### Verify Container Registry Access
```bash
# Test container registry login
docker login dhsairecr.azurecr.io

# List repositories
az acr repository list --name dhsairecr --output table
```

### Validate Secret Access
```bash
# List secrets in shared Key Vault
az keyvault secret list --vault-name dhs-aire-shared-kv --output table

# Check specific secret
az keyvault secret show --vault-name dhs-aire-shared-kv --name "tagmgmt-sql-connection-string-dev"
```

## Security Considerations

### Medical Device Compliance
- **Production isolation**: Dedicated Key Vault for production maintains compliance
- **Audit trail**: All access logged in Azure Monitor
- **Access control**: Principle of least privilege enforced
- **Data residency**: All resources in Sweden Central for EU compliance

### Non-Production Security
- **Environment separation**: Secrets prefixed by environment
- **Access policies**: Container apps can only read secrets
- **Network security**: Key Vault allows Azure services by default
- **Monitoring**: Failed access attempts trigger alerts

## Support & Documentation

### Related Documentation
- [DHS-AIRE Shared Infrastructure README](../../../dhs-aire-infrastructure/README.md)
- [Azure Key Vault Consolidation Strategy](./azure-keyvault-consolidation.md)
- [Resource Cleanup Summary](./resource-cleanup-summary.md)

### Troubleshooting
- **Secret access issues**: Verify access policies and container app identity
- **Container registry issues**: Check ACR permissions and image names
- **Naming conflicts**: Ensure secret names follow the agreed convention
- **Terraform errors**: Verify shared infrastructure is deployed first

---

## 🎯 Integration Summary

The Tag Management service is now fully integrated with the DHS-AIRE shared infrastructure, providing:

- ✅ **Consistent naming** across all DHS-AIRE projects
- ✅ **Cost optimization** through resource sharing
- ✅ **Operational efficiency** with centralized management
- ✅ **Maintained security** with proper access controls
- ✅ **Medical device compliance** with production isolation

**Next**: Deploy the shared Key Vault and test the integration in the development environment.
