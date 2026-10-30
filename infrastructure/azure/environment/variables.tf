# Set per environment in tfvars/<workspace>.tfvars (committed) and tfvars/<workspace>.entra.tfvars (git-ignored).
# The settings have no default on purpose: each environment's file states all of them.

# The Terraform state storage (created by hand), for terraform_remote_state. From TF_VAR_tfstate_resource_group_name
# and TF_VAR_tfstate_storage_account_name (README, "Environment variables").
variable "tfstate_resource_group_name" {
  type = string
}

variable "tfstate_storage_account_name" {
  type = string
}

variable "location" {
  type    = string
  default = "francecentral"
}

# Set by the infra-apply and infra-destroy workflows only (README, "From GitHub Actions").
variable "deployer_principal_name" {
  description = "Name of the service principal running Terraform. Null (local runs): the signed-in person."
  type        = string
  default     = null
}

variable "environment" {
  description = "The environment the variable files are written for. Must be the selected workspace (checked)."
  type        = string
}

variable "app_service_sku" {
  description = "B1 has no deployment slots; S1 or higher is needed for staging-slot swaps."
  type        = string
}

variable "postgres_sku" {
  type = string
}

variable "postgres_storage_mb" {
  type = number
}

variable "key_vault_purge_protection" {
  description = "When true, a deleted vault, certificate or key can't be purged before the retention period ends."
  type        = bool
}

variable "key_vault_soft_delete_days" {
  type = number
}

variable "github_repository" {
  description = "Repository (owner/name) whose workflows may deploy this environment, from the GitHub environment of the same name."
  type        = string
  default     = "Seizemi/claim-api"
}

# Identifiers only, but kept out of git like every tenant value (spec rule 9).
variable "entra" {
  description = "External ID tenant values of this environment (ClaimApi README, 'Values to collect per tenant')."
  type = object({
    authority                = string
    tenant_id                = string
    tenant_domain            = string
    web_client_id            = string
    web_service_principal_id = string
    graph_client_id          = string
    # claim-terraform app registration of the tenant, for GitHub Actions. Null (local runs): the Azure CLI.
    terraform_client_id = optional(string)
  })

  # Hidden in plan/apply output (Actions logs are public, spec step 3.5); still in clear text in the state and the plan
  # file.
  sensitive = true
}
