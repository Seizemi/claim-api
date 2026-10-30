# One application environment per Terraform workspace: the workspace (staging, prod) picks the state and names the
# resources; tfvars/<workspace>.tfvars and tfvars/<workspace>.entra.tfvars give its settings (README, "Applying").

locals {
  environment = terraform.workspace
}

data "terraform_remote_state" "shared" {
  backend = "azurerm"
  config = {
    # Subscription from ARM_SUBSCRIPTION_ID, like the backend.
    resource_group_name  = var.tfstate_resource_group_name
    storage_account_name = var.tfstate_storage_account_name
    container_name       = "tfstate"
    key                  = "shared.tfstate"
    use_azuread_auth     = true
  }

  # Checked before anything is planned.
  lifecycle {
    precondition {
      condition     = local.environment != "default"
      error_message = "Select an environment first: terraform workspace select <staging|prod>."
    }
    precondition {
      condition     = var.environment == local.environment
      error_message = "Workspace '${local.environment}' but the variable files are for '${var.environment}': pass -var-file=tfvars/${local.environment}.tfvars -var-file=tfvars/${local.environment}.entra.tfvars."
    }
  }
}

module "environment" {
  source = "../../modules/app-environment"

  providers = {
    azurerm          = azurerm
    azuread          = azuread
    azuread.external = azuread.external
    random           = random
    time             = time
  }

  environment = local.environment
  location    = var.location

  deployer_principal_name = var.deployer_principal_name

  container_registry_id           = data.terraform_remote_state.shared.outputs.container_registry_id
  container_registry_login_server = data.terraform_remote_state.shared.outputs.container_registry_login_server
  log_analytics_workspace_id      = data.terraform_remote_state.shared.outputs.log_analytics_workspace_id

  app_service_sku     = var.app_service_sku
  postgres_sku        = var.postgres_sku
  postgres_storage_mb = var.postgres_storage_mb

  key_vault_purge_protection = var.key_vault_purge_protection
  key_vault_soft_delete_days = var.key_vault_soft_delete_days

  entra = var.entra

  github_repository  = var.github_repository
  github_environment = local.environment
}
