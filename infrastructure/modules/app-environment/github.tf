module "github_identity" {
  source = "../user-assigned-identity"

  name                = "id-${local.name}-github"
  resource_group_name = module.resource_group.name
  location            = var.location
  tags                = local.tags

  federated_credentials = {
    "github-${var.github_environment}" = {
      issuer  = "https://token.actions.githubusercontent.com"
      subject = "repo:${var.github_repository}:environment:${var.github_environment}"
    }
  }
}

# Push the image, then point this web app at it.
resource "azurerm_role_assignment" "github_acr_push" {
  scope                = var.container_registry_id
  role_definition_name = "AcrPush"
  principal_id         = module.github_identity.principal_id
}

# "az acr login" looks the registry up first, which AcrPush doesn't allow. Metadata only: no image, no credential.
resource "azurerm_role_assignment" "github_acr_reader" {
  scope                = var.container_registry_id
  role_definition_name = "Reader"
  principal_id         = module.github_identity.principal_id
}

resource "azurerm_role_assignment" "github_web_app" {
  scope                = module.web_app.id
  role_definition_name = "Website Contributor"
  principal_id         = module.github_identity.principal_id
}