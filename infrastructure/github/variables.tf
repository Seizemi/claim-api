# The Terraform state storage (created by hand), for terraform_remote_state. From TF_VAR_tfstate_resource_group_name
# and TF_VAR_tfstate_storage_account_name (README, "Environment variables").
variable "tfstate_resource_group_name" {
  type = string
}

variable "tfstate_storage_account_name" {
  type = string
}

variable "github_owner" {
  type    = string
  default = "Seizemi"
}

variable "repository" {
  description = "Repository whose workflows build and deploy the application."
  type        = string
  default     = "claim-api"
}

variable "environments" {
  description = "Environments applied in azure/environment (its workspaces). Add one only once its Azure resources exist."
  type        = list(string)
  default     = ["staging"]
}

variable "deployment_branch" {
  description = "Only workflows running on this branch can use the deployment environments (and so sign in to Azure)."
  type        = string
  default     = "main"
}
