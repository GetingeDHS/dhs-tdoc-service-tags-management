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
