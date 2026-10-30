variable "environment" {
  description = "Environment name used in resource names and tags (staging, prod)."
  type        = string
}

variable "location" {
  description = "Azure region of every resource."
  type        = string
}

variable "deployer_principal_name" {
  description = "Name of the service principal running Terraform (PostgreSQL role name). Null: a person, whose UPN is read from Entra."
  type        = string
  default     = null
}

# --- Shared resources (azure/shared outputs) ---

variable "container_registry_id" {
  description = "Registry the web app pulls from (AcrPull) and GitHub Actions pushes to (AcrPush)."
  type        = string
}

variable "container_registry_login_server" {
  type = string
}

variable "log_analytics_workspace_id" {
  description = "Workspace receiving Application Insights data and the diagnostic logs."
  type        = string
}

# --- Network ---

variable "vnet_address_space" {
  type    = string
  default = "10.10.0.0/16"
}

variable "app_subnet_prefix" {
  description = "Subnet of the App Service VNet integration (outbound traffic only)."
  type        = string
  default     = "10.10.1.0/24"
}

variable "postgres_subnet_prefix" {
  description = "Subnet the PostgreSQL server is injected into."
  type        = string
  default     = "10.10.2.0/24"
}

# --- Compute and data ---

variable "app_service_sku" {
  description = "App Service plan SKU. B1 has no deployment slots; S1 or higher is needed for staging-slot swaps."
  type        = string
  default     = "B1"
}

variable "image_repository" {
  description = "Repository of the application image in the registry."
  type        = string
  default     = "claimapi"
}

variable "postgres_version" {
  description = "PostgreSQL major version (same as the local docker-compose)."
  type        = string
  default     = "16"
}

variable "postgres_sku" {
  type    = string
  default = "B_Standard_B1ms"
}

variable "postgres_storage_mb" {
  type    = number
  default = 32768
}

variable "postgres_database" {
  type    = string
  default = "claimapi"
}

variable "key_vault_purge_protection" {
  description = "When true, a deleted vault, certificate or key can't be purged before the retention period ends (prod)."
  type        = bool
  default     = true
}

variable "key_vault_soft_delete_days" {
  type    = number
  default = 90
}

# --- Application settings: Entra External ID tenant (identifiers, not secrets) ---

variable "entra" {
  description = "Values of the Entra External ID tenant used by this environment (README, 'Values to collect per tenant')."
  type = object({
    authority                = string # https://<subdomain>.ciamlogin.com/
    tenant_id                = string
    tenant_domain            = string # <subdomain>.onmicrosoft.com
    web_client_id            = string # partner-portal-web
    web_service_principal_id = string # partner-portal-web, Enterprise applications > Object ID
    graph_client_id          = string # partner-portal-graph
    # Used by the root's azuread.external provider only (claim-terraform app registration, GitHub Actions).
    terraform_client_id = optional(string)
  })
}

# --- GitHub Actions ---

variable "github_repository" {
  description = "Repository (owner/name) whose workflows may deploy this environment."
  type        = string
}

variable "github_environment" {
  description = "GitHub environment the deployment job runs in: part of the federated credential's subject."
  type        = string
}
