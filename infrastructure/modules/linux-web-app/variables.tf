variable "name" {
  description = "Globally unique: also the <name>.azurewebsites.net host name."
  type        = string
}

variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "service_plan_id" {
  type = string
}

variable "container_registry_login_server" {
  description = "Registry the image is pulled from with the managed identity (grant it AcrPull)."
  type        = string
}

variable "docker_image_name" {
  description = "Initial image (repository:tag). Ignored afterwards: deployments set the tag."
  type        = string
}

variable "app_settings" {
  type    = map(string)
  default = {}
}

variable "virtual_network_subnet_id" {
  description = "Subnet of the VNet integration (delegated to Microsoft.Web/serverFarms), or null."
  type        = string
  default     = null
}

variable "vnet_route_all_enabled" {
  description = "false: only private (RFC 1918) traffic goes through the VNet."
  type        = bool
  default     = false
}

variable "always_on" {
  type    = bool
  default = true
}

variable "client_affinity_enabled" {
  description = "Sticky sessions. Off when any instance can serve any user."
  type        = bool
  default     = false
}

variable "health_check_path" {
  type    = string
  default = "/healthz"
}

variable "health_check_eviction_time_in_min" {
  type    = number
  default = 2
}

variable "tags" {
  type    = map(string)
  default = {}
}
