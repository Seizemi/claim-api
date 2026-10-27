# GitHub environments of the claim-api repository and the values its workflows need to deploy there, taken from the
# Azure outputs. They're identifiers, not secrets: the workflows sign in to Azure through OpenID Connect federation,
# so no password exists. Hence variables (readable in GitHub and in the state), never secrets.

locals {
  # Subscription from ARM_SUBSCRIPTION_ID, like the backend.
  state_backend = {
    resource_group_name  = var.tfstate_resource_group_name
    storage_account_name = var.tfstate_storage_account_name
    container_name       = "tfstate"
    use_azuread_auth     = true
  }
}

data "terraform_remote_state" "shared" {
  backend = "azurerm"
  config  = merge(local.state_backend, { key = "shared.tfstate" })
}

# azure/environment, one workspace per environment.
data "terraform_remote_state" "environment" {
  for_each  = toset(var.environments)
  backend   = "azurerm"
  workspace = each.key
  config    = merge(local.state_backend, { key = "environment.tfstate" })
}

# The federated credential of each environment trusts jobs of the GitHub environment of the same name only
# (subject repo:<owner>/<repo>:environment:<name>).
resource "github_repository_environment" "this" {
  for_each    = toset(var.environments)
  repository  = var.repository
  environment = each.key

  deployment_branch_policy {
    protected_branches     = false
    custom_branch_policies = true
  }
}

resource "github_repository_environment_deployment_policy" "this" {
  for_each       = github_repository_environment.this
  repository     = var.repository
  environment    = each.value.environment
  branch_pattern = var.deployment_branch
}

locals {
  shared = data.terraform_remote_state.shared.outputs

  # One entry per environment and variable: "<environment>/<variable>" => { environment, name, value }.
  variables = merge([
    for environment, state in data.terraform_remote_state.environment : {
      for name, value in {
        AZURE_CLIENT_ID       = state.outputs.github_client_id
        AZURE_TENANT_ID       = state.outputs.tenant_id
        AZURE_SUBSCRIPTION_ID = state.outputs.subscription_id
        ACR_NAME              = local.shared.container_registry_name
        ACR_LOGIN_SERVER      = local.shared.container_registry_login_server
        RESOURCE_GROUP        = state.outputs.resource_group_name
        WEBAPP_NAME           = state.outputs.web_app_name
        PUBLIC_URL            = state.outputs.public_url
      } : "${environment}/${name}" => { environment = environment, name = name, value = value }
    }
  ]...)
}

resource "github_actions_environment_variable" "this" {
  for_each      = local.variables
  repository    = var.repository
  environment   = github_repository_environment.this[each.value.environment].environment
  variable_name = each.value.name
  value         = each.value.value
}
