variable "name" {
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "os_type" {
  type    = string
  default = "Linux"
}

variable "sku_name" {
  description = "B1 has no deployment slots; S1 or higher is needed for staging-slot swaps."
  type        = string
  default     = "B1"
}

variable "tags" {
  type    = map(string)
  default = {}
}
