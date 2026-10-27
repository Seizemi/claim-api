variable "name" {
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "security_rules" {
  description = "Rules by name."
  type = map(object({
    priority                   = number
    direction                  = string # Inbound | Outbound
    access                     = string # Allow | Deny
    protocol                   = optional(string, "*")
    source_address_prefix      = optional(string, "*")
    source_port_range          = optional(string, "*")
    destination_address_prefix = optional(string, "*")
    destination_port_range     = optional(string, "*")
  }))
  default = {}
}

variable "subnet_ids" {
  description = "Subnets the group is attached to, by a static key (the IDs are unknown until the subnets exist)."
  type        = map(string)
  default     = {}
}

variable "tags" {
  type    = map(string)
  default = {}
}
