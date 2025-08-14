terraform {
  required_version = ">=1.0"
  
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~>3.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~>3.1"
    }
  }

  backend "azurerm" {
    resource_group_name  = "rg-terraform-state-sweden"
    storage_account_name = "stterraformstatesweden"
    container_name       = "tfstate"
    key                  = "test-environment.tfstate"
  }
}

provider "azurerm" {
  features {}
}

locals {
  environment = "test"
  project     = "tdoc-tags"
  location    = "Sweden Central"
  
  tags = {
    Environment = local.environment
    Project     = local.project
    ManagedBy   = "Terraform"
    Purpose     = "E2E Testing"
  }
}

resource "random_string" "unique_suffix" {
  length  = 8
  special = false
  upper   = false
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "rg-${local.project}-${local.environment}-${random_string.unique_suffix.result}"
  location = local.location
  tags     = local.tags
}

# Use shared SQL Server from production infrastructure
# No need to create a new SQL Server - use the shared one

# SQL Database (Serverless for cost-effective testing)
resource "azurerm_mssql_database" "main" {
  name           = "sqldb-${local.project}-${local.environment}-${random_string.unique_suffix.result}"
  server_id      = var.shared_sql_server_id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  
  # Basic SKU for simple, reliable test environment
  sku_name = "Basic"
  max_size_gb = 2
  
  # Basic backup settings for test environment
  short_term_retention_policy {
    retention_days = 7  # Minimal retention for test environment
  }
  
  # No long-term retention needed for test databases
  
  tags = local.tags
}

# Note: Firewall rules are already configured on the shared SQL Server
# No need to add additional firewall rules here

# App Service Plan
resource "azurerm_service_plan" "main" {
  name                = "asp-${local.project}-${local.environment}-${random_string.unique_suffix.result}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  os_type             = "Linux"
  sku_name            = "B1"
  
  tags = local.tags
}

# App Service
resource "azurerm_linux_web_app" "main" {
  name                = "app-${local.project}-${local.environment}-${random_string.unique_suffix.result}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_service_plan.main.location
  service_plan_id     = azurerm_service_plan.main.id
  
  site_config {
    always_on = false
    
    application_stack {
      dotnet_version = "8.0"
    }
    
    cors {
      allowed_origins = ["*"]
    }
  }
  
  app_settings = {
    "ASPNETCORE_ENVIRONMENT" = "Testing"
    "Logging__LogLevel__Default" = "Information"
    "AllowedHosts" = "*"
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = azurerm_application_insights.main.connection_string
    
    # Database connection components - app will build connection string at runtime
    "DATABASE_NAME" = azurerm_mssql_database.main.name
    "SQL_SERVER_FQDN" = "@Microsoft.KeyVault(VaultName=${var.shared_key_vault_name};SecretName=tagmgmt-sql-server-fqdn)"
    "SQL_USERNAME" = "@Microsoft.KeyVault(VaultName=${var.shared_key_vault_name};SecretName=tagmgmt-sql-admin-username)"
    "SQL_PASSWORD" = "@Microsoft.KeyVault(VaultName=${var.shared_key_vault_name};SecretName=tagmgmt-sql-admin-password)"
  }
  
  identity {
    type = "SystemAssigned"
  }
  
  tags = local.tags
}

# Application Insights
resource "azurerm_application_insights" "main" {
  name                = "appi-${local.project}-${local.environment}-${random_string.unique_suffix.result}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  application_type    = "web"
  
  lifecycle {
    ignore_changes = [workspace_id]
  }
  
  tags = local.tags
}

data "azurerm_client_config" "current" {}

# Reference to shared Key Vault from production infrastructure
data "azurerm_key_vault" "shared" {
  name                = var.shared_key_vault_name
  resource_group_name = var.shared_resource_group_name
}

# Get Tag Management SQL Server credentials from shared Key Vault
data "azurerm_key_vault_secret" "tagmgmt_sql_server_fqdn" {
  name         = "tagmgmt-sql-server-fqdn"
  key_vault_id = data.azurerm_key_vault.shared.id
}

data "azurerm_key_vault_secret" "tagmgmt_sql_admin_username" {
  name         = "tagmgmt-sql-admin-username"
  key_vault_id = data.azurerm_key_vault.shared.id
}

data "azurerm_key_vault_secret" "tagmgmt_sql_admin_password" {
  name         = "tagmgmt-sql-admin-password"
  key_vault_id = data.azurerm_key_vault.shared.id
}

# Grant the App Service access to the shared Key Vault
resource "azurerm_key_vault_access_policy" "app_service" {
  key_vault_id = data.azurerm_key_vault.shared.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_linux_web_app.main.identity[0].principal_id

  secret_permissions = ["Get", "List"]
}

# Note: Using shared connection string template from Key Vault
# The application will substitute {DATABASE_NAME} and {SQL_PASSWORD} placeholders
# with values from app settings


# Outputs
output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.main.name
}

output "app_service_name" {
  description = "Name of the app service"
  value       = azurerm_linux_web_app.main.name
}

output "app_service_url" {
  description = "URL of the app service"
  value       = "https://${azurerm_linux_web_app.main.default_hostname}"
}

output "sql_server_fqdn" {
  description = "FQDN of the shared SQL server"
  value       = data.azurerm_key_vault_secret.tagmgmt_sql_server_fqdn.value
  sensitive   = true
}

output "sql_database_name" {
  description = "Name of the test SQL database"
  value       = azurerm_mssql_database.main.name
}

output "connection_string_template" {
  description = "Shared connection string template from Key Vault"
  value       = "tagmgmt-test-connection-string"
}

output "key_vault_secret_name" {
  description = "Name of the shared Key Vault secret containing the connection string template"
  value       = "tagmgmt-test-connection-string"
}
