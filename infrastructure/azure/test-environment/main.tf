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

# SQL Server
resource "azurerm_mssql_server" "main" {
  name                         = "sql-${local.project}-${local.environment}-${random_string.unique_suffix.result}"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_username
  administrator_login_password = var.sql_admin_password
  
  tags = local.tags
}

# SQL Database
resource "azurerm_mssql_database" "main" {
  name           = "sqldb-${local.project}-${local.environment}"
  server_id      = azurerm_mssql_server.main.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  license_type   = "LicenseIncluded"
  max_size_gb    = 2
  sku_name       = "S0"
  
  tags = local.tags
}

# SQL Server Firewall Rules
resource "azurerm_mssql_firewall_rule" "allow_azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_mssql_firewall_rule" "allow_github_actions" {
  name             = "AllowGitHubActions"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "255.255.255.255"
}

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
    "ConnectionStrings__DefaultConnection" = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.main.name};Persist Security Info=False;User ID=${var.sql_admin_username};Password=${var.sql_admin_password};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    "ASPNETCORE_ENVIRONMENT" = "Testing"
    "Logging__LogLevel__Default" = "Information"
    "AllowedHosts" = "*"
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
  
  tags = local.tags
}

# Key Vault for storing secrets
resource "azurerm_key_vault" "main" {
  name                = "kv-${substr(replace("${local.project}-${random_string.unique_suffix.result}", "_", "-"), 0, 21)}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "standard"
  
  tags = local.tags
}

data "azurerm_client_config" "current" {}

# Key Vault Access Policy
resource "azurerm_key_vault_access_policy" "app_service" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_linux_web_app.main.identity[0].principal_id

  secret_permissions = ["Get", "List"]
}


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

output "sql_server_name" {
  description = "Name of the SQL server"
  value       = azurerm_mssql_server.main.name
}

output "sql_database_name" {
  description = "Name of the SQL database"
  value       = azurerm_mssql_database.main.name
}

output "connection_string" {
  description = "Database connection string"
  value       = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.main.name};Persist Security Info=False;User ID=${var.sql_admin_username};Password=${var.sql_admin_password};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  sensitive   = true
}
