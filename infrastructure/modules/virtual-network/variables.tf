variable "name" {
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "address_space" {
  type = list(string)
}

variable "subnets" {
  description = "Subnets by name, each optionally delegated to an Azure service."
  type = map(object({
    address_prefixes = list(string)
    delegation = optional(object({
      name    = string
      service = string # e.g. Microsoft.Web/serverFarms
      actions = list(string)
    }))
  }))
  default = {}
}

variable "tags" {
  type    = map(string)
  default = {}
}
