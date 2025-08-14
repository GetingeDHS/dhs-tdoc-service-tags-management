variable "sql_admin_username" {
  description = "Administrator username for the SQL Server"
  type        = string
  default     = "sqladmin"
  sensitive   = true
}

variable "sql_admin_password" {
  description = "Administrator password for the SQL Server"
  type        = string
  sensitive   = true
  
  validation {
    condition = length(var.sql_admin_password) >= 8
    error_message = "Password must be at least 8 characters long."
  }
}

# Shared infrastructure configuration
variable "shared_key_vault_name" {
  description = "Name of the shared Key Vault from production infrastructure"
  type        = string
}

variable "shared_resource_group_name" {
  description = "Name of the shared resource group containing shared resources"
  type        = string
}

variable "shared_sql_server_id" {
  description = "ID of the shared SQL Server from production infrastructure"
  type        = string
}
