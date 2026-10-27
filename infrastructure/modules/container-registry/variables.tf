variable "name" {
  description = "Globally unique, letters and digits only."
  type        = string
}

variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "sku" {
  type    = string
  default = "Basic"
}

variable "admin_enabled" {
  description = "Admin user (shared username/password). Keep it off and grant AcrPull/AcrPush to identities instead."
  type        = bool
  default     = false
}

variable "tags" {
  type    = map(string)
  default = {}
}
