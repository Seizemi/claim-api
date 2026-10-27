variable "name" {
  description = "Globally unique, 3 to 24 characters."
  type        = string
}

variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "tenant_id" {
  type = string
}

variable "sku_name" {
  type    = string
  default = "standard"
}

variable "purge_protection_enabled" {
  description = "When true, a deleted vault, certificate or key can't be purged before the retention period ends."
  type        = bool
  default     = true
}

variable "soft_delete_retention_days" {
  type    = number
  default = 90
}

variable "public_network_access_enabled" {
  type    = bool
  default = true
}

variable "administrator_principal_id" {
  description = "Granted Key Vault Administrator so that Terraform can manage the certificates and keys: the principal running it."
  type        = string
}

variable "rbac_propagation_wait" {
  description = "Wait after granting the administrator role, before creating certificates and keys."
  type        = string
  default     = "60s"
}

variable "certificates" {
  description = "Self-signed certificates, by a key of your choice."
  type = map(object({
    name                     = string
    subject                  = string # CN=...
    validity_in_months       = optional(number, 12)
    renew_days_before_expiry = optional(number, 30)
    key_size                 = optional(number, 2048)
    exportable               = optional(bool, true) # the app loads the private key (read as a Key Vault secret)
  }))
  default = {}
}

variable "keys" {
  description = "Keys, by a key of your choice."
  type = map(object({
    name     = string
    key_type = optional(string, "RSA")
    key_size = optional(number, 2048)
    key_opts = list(string)
  }))
  default = {}
}

variable "tags" {
  type    = map(string)
  default = {}
}
