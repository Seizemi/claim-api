variable "name" {
  description = "Globally unique, 3 to 24 lowercase letters and digits."
  type        = string
}

variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "account_tier" {
  type    = string
  default = "Standard"
}

variable "account_replication_type" {
  type    = string
  default = "LRS"
}

variable "containers" {
  description = "Names of the private blob containers."
  type        = list(string)
  default     = []
}

variable "tags" {
  type    = map(string)
  default = {}
}
