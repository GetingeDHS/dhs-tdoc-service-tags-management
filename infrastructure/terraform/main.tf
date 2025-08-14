# Configure Terraform and required providers
terraform {
  required_version = ">= 1.5"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.1"
    }
  }
  
  # Remote state configuration for team collaboration
  backend "azurerm" {
    # These will be configured during terraform init
    # resource_group_name  = "tfstate-rg"
    # storage_account_name = "tfstate<random>"
    # container_name       = "tfstate"
    # key                  = "tag-management.tfstate"
  }
}

# Configure Azure Provider
provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
    key_vault {
      purge_soft_delete_on_destroy    = true
      recover_soft_deleted_key_vaults = true
    }
  }
}

# Data sources
data "azurerm_client_config" "current" {}

# Random suffix for unique resource names
resource "random_string" "suffix" {
  length  = 6
  special = false
  upper   = false
}

# Local variables
locals {
  environment = var.environment
  location    = var.location
  
  # Naming convention for medical device compliance
  prefix = "tagmgmt-${local.environment}"
  suffix = random_string.suffix.result
  
  # Common tags for all resources (medical device compliance)
  common_tags = {
    Environment         = local.environment
    Project            = "TagManagement"
    ManagedBy          = "Terraform"
    Owner              = var.owner
    CostCenter         = var.cost_center
    MedicalDevice      = "true"
    Compliance         = "ISO-13485"
    Classification     = "Class-II"
    BackupRequired     = "true"
    MonitoringRequired = "true"
  }
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "${local.prefix}-rg-${local.suffix}"
  location = local.location
  tags     = local.common_tags
}

# Log Analytics Workspace for monitoring (medical device requirement)
resource "azurerm_log_analytics_workspace" "main" {
  name                = "${local.prefix}-law-${local.suffix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = var.log_retention_days
  
  tags = local.common_tags
}

# Application Insights for APM
resource "azurerm_application_insights" "main" {
  name                = "${local.prefix}-ai-${local.suffix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "web"
  
  tags = local.common_tags
}

# Reference to shared Key Vault from dhs-aire-infrastructure
data "azurerm_key_vault" "shared" {
  name                = var.shared_key_vault_name
  resource_group_name = var.shared_resource_group_name
}

# Access policy for this project's service principal to access shared Key Vault
resource "azurerm_key_vault_access_policy" "tagmanagement_terraform" {
  key_vault_id = data.azurerm_key_vault.shared.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id
  
  secret_permissions = [
    "Get", "List", "Set", "Delete", "Purge", "Recover"
  ]
}

# Lightweight SQL Server optimized for serverless databases only
# This has minimal overhead and only serves as a container for serverless databases
resource "azurerm_mssql_server" "main" {
  name                         = "${local.prefix}-sql-${local.suffix}"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_username
  administrator_login_password = var.sql_admin_password
  
  # Security configurations for medical device compliance
  minimum_tls_version = "1.2"
  
  # Disable AAD admin for POC to reduce complexity and cost
  # azuread_administrator can be added later if needed
  
  tags = merge(local.common_tags, {
    Purpose = "Serverless Database Host"
    Note    = "Lightweight server for serverless databases only"
  })
}

# SQL Server Firewall Rule for Azure Services
resource "azurerm_mssql_firewall_rule" "azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# SQL Database (Serverless for cost-effective POC)
resource "azurerm_mssql_database" "main" {
  name         = "TDOC"
  server_id    = azurerm_mssql_server.main.id
  collation    = "SQL_Latin1_General_CP1_CI_AS"
  
  # Serverless configuration - lowest tier for POC
  sku_name                    = "GP_S_Gen5_1"
  auto_pause_delay_in_minutes = 60    # Auto-pause after 1 hour of inactivity
  min_capacity                = 0.5   # Minimum vCores (lowest possible)
  max_capacity                = 1     # Maximum vCores (keep low for POC)
  
  # Basic backup settings for POC (reduced from production settings)
  short_term_retention_policy {
    retention_days = 7  # Reduced from 35 for POC
  }
  
  # Skip long-term retention for POC to reduce costs
  # long_term_retention_policy can be added later if needed
  
  tags = local.common_tags
}

# Note: SQL Server auditing removed for POC to reduce costs and complexity
# For production deployment, uncomment and configure auditing as needed

# Container Registry for microservice images
resource "azurerm_container_registry" "main" {
  name                = "${replace(local.prefix, "-", "")}acr${local.suffix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "Premium"
  admin_enabled       = false
  
  # Security scanning for medical device compliance
  quarantine_policy_enabled = true
  trust_policy_enabled      = true
  retention_policy_enabled  = true
  
  tags = local.common_tags
}

# Container App Environment
resource "azurerm_container_app_environment" "main" {
  name                       = "${local.prefix}-cae-${local.suffix}"
  location                   = azurerm_resource_group.main.location
  resource_group_name        = azurerm_resource_group.main.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id
  
  tags = local.common_tags
}

# Container App for Tag Management Service
resource "azurerm_container_app" "tagmanagement" {
  name                         = "${local.prefix}-ca-${local.suffix}"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = "Single"
  
  template {
    min_replicas = var.min_replicas
    max_replicas = var.max_replicas
    
    container {
      name   = "tagmanagement-api"
      image  = "${azurerm_container_registry.main.login_server}/tagmanagement:latest"
      cpu    = var.container_cpu
      memory = var.container_memory
      
      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = title(local.environment)
      }
      
      env {
        name  = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        value = azurerm_application_insights.main.connection_string
      }
      
      env {
        name        = "ConnectionStrings__DefaultConnection"
        secret_name = "database-connection-string"
      }
      
      # Health probes for medical device monitoring
      liveness_probe {
        http_get {
          path = "/health"
          port = 8080
        }
        initial_delay_seconds = 30
        period_seconds        = 10
        timeout_seconds       = 5
        failure_threshold     = 3
      }
      
      readiness_probe {
        http_get {
          path = "/health/ready"
          port = 8080
        }
        initial_delay_seconds = 5
        period_seconds        = 5
        timeout_seconds       = 3
        failure_threshold     = 3
      }
    }
    
    # Auto-scaling based on CPU and memory
    scale {
      min_replicas = var.min_replicas
      max_replicas = var.max_replicas
      
      rule {
        name = "cpu-scaling"
        cpu {
          utilization = 70
        }
      }
      
      rule {
        name = "memory-scaling"
        memory {
          utilization = 80
        }
      }
    }
  }
  
  secret {
    name  = "database-connection-string"
    value = "Server=${azurerm_mssql_server.main.fully_qualified_domain_name};Database=${azurerm_mssql_database.main.name};User Id=${var.sql_admin_username};Password=${var.sql_admin_password};TrustServerCertificate=true;MultipleActiveResultSets=true"
  }
  
  ingress {
    allow_insecure_connections = false
    external_enabled          = true
    target_port               = 8080
    
    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }
  
  identity {
    type = "SystemAssigned"
  }
  
  tags = local.common_tags
}

# Action Group for alerts (medical device monitoring requirement)
resource "azurerm_monitor_action_group" "main" {
  name                = "${local.prefix}-ag-${local.suffix}"
  resource_group_name = azurerm_resource_group.main.name
  short_name          = "tagmgmt"
  
  email_receiver {
    name          = "operations"
    email_address = var.operations_email
  }
  
  sms_receiver {
    name         = "oncall"
    country_code = var.sms_country_code
    phone_number = var.oncall_phone_number
  }
  
  tags = local.common_tags
}

# Critical alerts for medical device compliance
resource "azurerm_monitor_metric_alert" "high_error_rate" {
  name                = "${local.prefix}-high-error-rate-${local.suffix}"
  resource_group_name = azurerm_resource_group.main.name
  scopes              = [azurerm_container_app.tagmanagement.id]
  description         = "Alert when error rate is high"
  severity            = 1
  frequency           = "PT1M"
  window_size         = "PT5M"
  
  criteria {
    metric_namespace = "Microsoft.Web/sites"
    metric_name      = "Http5xx"
    aggregation      = "Total"
    operator         = "GreaterThan"
    threshold        = 10
  }
  
  action {
    action_group_id = azurerm_monitor_action_group.main.id
  }
  
  tags = local.common_tags
}

# Database connectivity alert
resource "azurerm_monitor_metric_alert" "database_connection_failed" {
  name                = "${local.prefix}-db-connection-failed-${local.suffix}"
  resource_group_name = azurerm_resource_group.main.name
  scopes              = [azurerm_mssql_database.main.id]
  description         = "Alert when database connections are failing"
  severity            = 0
  frequency           = "PT1M"
  window_size         = "PT5M"
  
  criteria {
    metric_namespace = "Microsoft.Sql/servers/databases"
    metric_name      = "connection_failed"
    aggregation      = "Total"
    operator         = "GreaterThan"
    threshold        = 5
  }
  
  action {
    action_group_id = azurerm_monitor_action_group.main.id
  }
  
  tags = local.common_tags
}

# Store Tag Management SQL Server information in shared Key Vault for PR environments
resource "azurerm_key_vault_secret" "tagmgmt_sql_server_fqdn" {
  name         = "tagmgmt-sql-server-fqdn"
  value        = azurerm_mssql_server.main.fully_qualified_domain_name
  key_vault_id = data.azurerm_key_vault.shared.id
  
  depends_on = [
    azurerm_key_vault_access_policy.tagmanagement_terraform
  ]
  
  tags = local.common_tags
}

resource "azurerm_key_vault_secret" "tagmgmt_sql_admin_username" {
  name         = "tagmgmt-sql-admin-username"
  value        = var.sql_admin_username
  key_vault_id = data.azurerm_key_vault.shared.id
  
  depends_on = [
    azurerm_key_vault_access_policy.tagmanagement_terraform
  ]
  
  tags = local.common_tags
}

resource "azurerm_key_vault_secret" "tagmgmt_sql_admin_password" {
  name         = "tagmgmt-sql-admin-password"
  value        = var.sql_admin_password
  key_vault_id = data.azurerm_key_vault.shared.id
  
  depends_on = [
    azurerm_key_vault_access_policy.tagmanagement_terraform
  ]
  
  tags = local.common_tags
}

# Outputs for shared resources (SQL Server and Key Vault)
output "shared_sql_server_id" {
  description = "ID of the shared SQL Server for PR test environments"
  value       = azurerm_mssql_server.main.id
}

output "shared_sql_server_name" {
  description = "Name of the shared SQL Server for PR test environments"
  value       = azurerm_mssql_server.main.name
}

output "shared_sql_server_fqdn" {
  description = "FQDN of the shared SQL Server for PR test environments"
  value       = azurerm_mssql_server.main.fully_qualified_domain_name
  sensitive   = true
}

output "shared_key_vault_id" {
  description = "ID of the shared Key Vault for PR test environments"
  value       = data.azurerm_key_vault.shared.id
}

output "shared_key_vault_name" {
  description = "Name of the shared Key Vault for PR test environments"
  value       = data.azurerm_key_vault.shared.name
}

output "shared_key_vault_vault_uri" {
  description = "URI of the shared Key Vault for PR test environments"
  value       = data.azurerm_key_vault.shared.vault_uri
}

output "shared_key_vault_resource_group_name" {
  description = "Name of the resource group containing shared Key Vault"
  value       = data.azurerm_key_vault.shared.resource_group_name
}
