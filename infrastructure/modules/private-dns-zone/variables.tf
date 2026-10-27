variable "name" {
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "virtual_network_links" {
  description = "VNets resolving names through this zone: link name => VNet ID."
  type        = map(string)
  default     = {}
}

variable "tags" {
  type    = map(string)
  default = {}
}
