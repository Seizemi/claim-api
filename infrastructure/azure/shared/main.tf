# Resources used by every environment.

locals {
  tags = {
    project     = "claim"
    environment = "shared"
    managed-by  = "terraform"
  }
}

module "resource_group" {
  source = "../../modules/resource-group"

  name     = "rg-claim-shared-frc"
  location = var.location
  tags     = local.tags
}

# Registry names are global and letters/digits only.
resource "random_string" "suffix" {
  length  = 5
  upper   = false
  special = false
}

# One registry for every environment: the image tested in staging is the one deployed to prod.
# No admin user: App Services pull with their managed identity (AcrPull), GitHub Actions push with its federated
# identity (AcrPush). Both roles are granted by each environment.
module "container_registry" {
  source = "../../modules/container-registry"

  name                = "acrclaim${random_string.suffix.result}"
  resource_group_name = module.resource_group.name
  location            = var.location
  tags                = local.tags
}

# Logs of every environment (App Service, PostgreSQL, Application Insights).
module "log_analytics_workspace" {
  source = "../../modules/log-analytics-workspace"

  name                = "log-claim-shared-frc"
  resource_group_name = module.resource_group.name
  location            = var.location
  retention_in_days   = var.log_retention_days
  tags                = local.tags
}
