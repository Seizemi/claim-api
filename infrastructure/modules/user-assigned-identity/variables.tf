variable "name" {
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "federated_credentials" {
  description = "Federated credentials by name."
  type = map(object({
    issuer   = string
    subject  = string
    audience = optional(list(string), ["api://AzureADTokenExchange"])
  }))
  default = {}
}

variable "tags" {
  type    = map(string)
  default = {}
}
