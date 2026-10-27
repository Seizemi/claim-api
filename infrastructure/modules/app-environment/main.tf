# One application environment: network, PostgreSQL, Key Vault, Data Protection storage, monitoring, the web app and
# the identity GitHub Actions deploys with. Shared resources (registry, Log Analytics) come from azure/shared.
# This module only names and wires the resource modules (../<resource>) together and grants the roles between them.

data "azurerm_client_config" "current" {}

# The principal running Terraform (the deployer): Key Vault administrator and PostgreSQL Entra administrator.
# A person signed in with the Azure CLI is looked up in Entra for their UPN; the GitHub Actions Terraform identity
# passes its name instead (it has no right to read the directory).
data "azuread_user" "current" {
  count     = var.deployer_principal_name == null ? 1 : 0
  object_id = data.azurerm_client_config.current.object_id
}

locals {
  deployer = {
    object_id      = data.azurerm_client_config.current.object_id
    principal_name = var.deployer_principal_name != null ? var.deployer_principal_name : data.azuread_user.current[0].user_principal_name
    principal_type = var.deployer_principal_name != null ? "ServicePrincipal" : "User"
  }

  name = "claim-${var.environment}-frc"

  tags = {
    project     = "claim"
    environment = var.environment
    managed-by  = "terraform"
  }
}

module "resource_group" {
  source = "../resource-group"

  name     = "rg-${local.name}"
  location = var.location
  tags     = local.tags
}

# Storage account names are global and letters/digits only.
resource "random_string" "suffix" {
  length  = 5
  upper   = false
  special = false
}
