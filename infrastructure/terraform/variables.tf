# Environment Configuration
variable "environment" {
  description = "The deployment environment (dev, staging, prod)"
  type        = string
  default     = "dev"
  
  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be one of: dev, staging, prod."
  }
}

variable "location" {
  description = "The Azure region where resources will be deployed"
  type        = string
  default     = "East US 2"
}

# Ownership and Compliance
variable "owner" {
  description = "The owner or team responsible for the resources"
  type        = string
  default     = "Medical Device Engineering"
}

variable "cost_center" {
  description = "Cost center for billing and resource allocation"
  type        = string
  default     = "MD-ENG-001"
}

# Database Configuration
variable "sql_admin_username" {
  description = "SQL Server administrator username"
  type        = string
  default     = "tdoc_admin"
  sensitive   = true
}

variable "sql_admin_password" {
  description = "SQL Server administrator password"
  type        = string
  sensitive   = true
  
  validation {
    condition = can(regex("^.{12,128}$", var.sql_admin_password)) && can(regex("[A-Z]", var.sql_admin_password)) && can(regex("[a-z]", var.sql_admin_password)) && can(regex("[0-9]", var.sql_admin_password)) && can(regex("[^A-Za-z0-9]", var.sql_admin_password))
    error_message = "Password must be 12-128 characters and contain uppercase, lowercase, digit, and special character."
  }
}

variable "sql_aad_admin_login" {
  description = "Azure AD administrator login for SQL Server"
  type        = string
  default     = "sql-admins@yourcompany.com"
}

variable "sql_aad_admin_object_id" {
  description = "Azure AD administrator object ID for SQL Server"
  type        = string
}

variable "sql_database_sku" {
  description = "SQL Database SKU"
  type        = string
  default     = "S2"
  
  validation {
    condition = contains([
      "Basic", "S0", "S1", "S2", "S3", "S4", "S6", "S7", "S9", "S12",
      "P1", "P2", "P4", "P6", "P11", "P15",
      "GP_S_Gen5_1", "GP_S_Gen5_2", "GP_S_Gen5_4", "GP_S_Gen5_8",
      "GP_Gen5_2", "GP_Gen5_4", "GP_Gen5_8", "GP_Gen5_16", "GP_Gen5_24", "GP_Gen5_32", "GP_Gen5_40", "GP_Gen5_80",
      "BC_Gen5_2", "BC_Gen5_4", "BC_Gen5_8", "BC_Gen5_16", "BC_Gen5_24", "BC_Gen5_32", "BC_Gen5_40", "BC_Gen5_80"
    ], var.sql_database_sku)
    error_message = "Invalid SQL Database SKU provided."
  }
}

# Container App Configuration
variable "container_cpu" {
  description = "CPU allocation for container app"
  type        = number
  default     = 0.5
}

variable "container_memory" {
  description = "Memory allocation for container app"
  type        = string
  default     = "1Gi"
}

variable "min_replicas" {
  description = "Minimum number of container replicas"
  type        = number
  default     = 2
  
  validation {
    condition     = var.min_replicas >= 1
    error_message = "Minimum replicas must be at least 1."
  }
}

variable "max_replicas" {
  description = "Maximum number of container replicas"
  type        = number
  default     = 10
  
  validation {
    condition     = var.max_replicas >= var.min_replicas
    error_message = "Maximum replicas must be greater than or equal to minimum replicas."
  }
}

# Monitoring and Compliance
variable "log_retention_days" {
  description = "Log Analytics workspace retention period in days (medical device compliance)"
  type        = number
  default     = 730  # 2 years for medical device compliance
  
  validation {
    condition     = var.log_retention_days >= 30 && var.log_retention_days <= 730
    error_message = "Log retention must be between 30 and 730 days."
  }
}

variable "audit_retention_days" {
  description = "SQL audit log retention period in days (medical device compliance)"
  type        = number
  default     = 2555  # 7 years for medical device compliance
  
  validation {
    condition     = var.audit_retention_days >= 90
    error_message = "Audit retention must be at least 90 days for compliance."
  }
}

# Security and Alerts
variable "security_contact_email" {
  description = "Security contact email for Key Vault notifications"
  type        = string
  default     = "security@yourcompany.com"
  
  validation {
    condition     = can(regex("^[^@]+@[^@]+\\.[^@]+$", var.security_contact_email))
    error_message = "Security contact email must be a valid email address."
  }
}

variable "operations_email" {
  description = "Operations team email for monitoring alerts"
  type        = string
  default     = "operations@yourcompany.com"
  
  validation {
    condition     = can(regex("^[^@]+@[^@]+\\.[^@]+$", var.operations_email))
    error_message = "Operations email must be a valid email address."
  }
}

variable "sms_country_code" {
  description = "Country code for SMS alerts (medical device critical alerts)"
  type        = string
  default     = "1"
}

variable "oncall_phone_number" {
  description = "On-call phone number for critical alerts (medical device compliance)"
  type        = string
  default     = "5551234567"
  
  validation {
    condition     = can(regex("^[0-9]+$", var.oncall_phone_number))
    error_message = "Phone number must contain only digits."
  }
}

# Network Security
variable "allowed_ip_ranges" {
  description = "List of IP ranges allowed to access resources"
  type        = list(string)
  default     = ["0.0.0.0/0"]  # Should be restricted in production
}

variable "enable_private_endpoints" {
  description = "Enable private endpoints for enhanced security"
  type        = bool
  default     = true
}

# Backup and Disaster Recovery
variable "backup_retention_days" {
  description = "Database backup retention period in days"
  type        = number
  default     = 35
  
  validation {
    condition     = var.backup_retention_days >= 7 && var.backup_retention_days <= 35
    error_message = "Backup retention must be between 7 and 35 days."
  }
}

variable "geo_redundant_backup" {
  description = "Enable geo-redundant backup for disaster recovery"
  type        = bool
  default     = true
}

# Performance and Scaling
variable "auto_pause_delay" {
  description = "Auto-pause delay for serverless SQL databases (minutes, -1 to disable)"
  type        = number
  default     = -1  # Disabled for medical device systems
}

variable "max_size_gb" {
  description = "Maximum size of the SQL database in GB"
  type        = number
  default     = 100
}

# Medical Device Specific
variable "fda_validation_required" {
  description = "Whether FDA validation processes are required"
  type        = bool
  default     = true
}

variable "hipaa_compliance_required" {
  description = "Whether HIPAA compliance is required"
  type        = bool
  default     = false
}

variable "iso_13485_required" {
  description = "Whether ISO 13485 compliance is required"
  type        = bool
  default     = true
}

variable "data_residency_region" {
  description = "Data residency region for compliance requirements"
  type        = string
  default     = "US"
  
  validation {
    condition     = contains(["US", "EU", "UK", "CA", "AU"], var.data_residency_region)
    error_message = "Data residency must be one of: US, EU, UK, CA, AU."
  }
}
