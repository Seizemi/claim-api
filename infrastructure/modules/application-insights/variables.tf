variable "name" {
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "log_analytics_workspace_id" {
  description = "Workspace the telemetry is stored in."
  type        = string
}

variable "application_type" {
  type    = string
  default = "web"
}

variable "tags" {
  type    = map(string)
  default = {}
}
