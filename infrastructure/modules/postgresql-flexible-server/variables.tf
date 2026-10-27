variable "name" {
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "postgres_version" {
  description = "PostgreSQL major version."
  type        = string
  default     = "16"
}

variable "sku_name" {
  type    = string
  default = "B_Standard_B1ms"
}

variable "storage_mb" {
  type    = number
  default = 32768
}

variable "backup_retention_days" {
  type    = number
  default = 7
}

variable "delegated_subnet_id" {
  description = "Subnet delegated to Microsoft.DBforPostgreSQL/flexibleServers."
  type        = string
}

variable "private_dns_zone_id" {
  description = "Private DNS zone (<name>.private.postgres.database.azure.com) linked to the subnet's VNet."
  type        = string
}

variable "tenant_id" {
  description = "Entra tenant the administrators belong to."
  type        = string
}

variable "administrators" {
  description = "Entra administrators; principal_name is the PostgreSQL role each connects as. The first is created alone, then the others."
  type = list(object({
    object_id      = string
    principal_name = string
    principal_type = string # User | Group | ServicePrincipal
  }))

  validation {
    condition     = length(var.administrators) > 0
    error_message = "At least one Entra administrator is needed: password authentication is disabled."
  }
}

variable "databases" {
  type    = list(string)
  default = []
}

variable "database_charset" {
  type    = string
  default = "UTF8"
}

variable "database_collation" {
  type    = string
  default = "en_US.utf8"
}

variable "tags" {
  type    = map(string)
  default = {}
}
