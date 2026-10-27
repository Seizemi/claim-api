variable "location" {
  description = "Azure region of every resource."
  type        = string
  default     = "francecentral"
}

variable "log_retention_days" {
  description = "How long Log Analytics keeps logs (30 days are included in the ingestion price)."
  type        = number
  default     = 30
}
