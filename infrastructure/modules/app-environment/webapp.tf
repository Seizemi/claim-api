# The container image (API + React build) on App Service for Linux. The system-assigned managed identity pulls the
# image from the registry and reads Key Vault, the Data Protection container and PostgreSQL: no password anywhere.

module "app_service_plan" {
  source = "../app-service-plan"

  name                = "asp-${local.name}"
  resource_group_name = module.resource_group.name
  location            = var.location
  sku_name            = var.app_service_sku
  tags                = local.tags
}

# Known before the web app exists: its own settings and the PostgreSQL role name need them.
locals {
  web_app_name = "app-${local.name}"
  public_url   = "https://${local.web_app_name}.azurewebsites.net"
}

module "web_app" {
  source = "../linux-web-app"

  name                            = local.web_app_name
  resource_group_name             = module.resource_group.name
  location                        = var.location
  service_plan_id                 = module.app_service_plan.id
  virtual_network_subnet_id       = module.virtual_network.subnet_ids["snet-app"]
  container_registry_login_server = var.container_registry_login_server
  # Placeholder until the first deployment: the pipeline sets the git-SHA tag.
  docker_image_name = "${var.image_repository}:latest"
  tags              = local.tags

  app_settings = {
    WEBSITES_PORT                       = "8080"
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
    ASPNETCORE_ENVIRONMENT              = "Production"
    # App Service terminates HTTPS: without the forwarded headers the app builds http:// redirect URIs.
    ASPNETCORE_FORWARDEDHEADERS_ENABLED = "true"

    App__PublicUrl = local.public_url

    AzureAd__Authority = var.entra.authority
    AzureAd__TenantId  = var.entra.tenant_id
    AzureAd__ClientId  = var.entra.web_client_id

    PartnerUsers__TenantId              = var.entra.tenant_id
    PartnerUsers__TenantDomain          = var.entra.tenant_domain
    PartnerUsers__GraphClientId         = var.entra.graph_client_id
    PartnerUsers__AppServicePrincipalId = var.entra.web_service_principal_id

    # Certificates of the two app registrations (step 3.3 wires the credentials to them).
    KeyVault__Uri                  = module.key_vault.vault_uri
    KeyVault__WebCertificateName   = module.key_vault.certificate_names["web"]
    KeyVault__GraphCertificateName = module.key_vault.certificate_names["graph"]

    # No password: the app gets an Entra token for its managed identity (username = the web app's name).
    ConnectionStrings__Postgres = join(";", [
      "Host=${module.postgres.fqdn}",
      "Database=${var.postgres_database}",
      "Username=${local.web_app_name}",
      "Ssl Mode=Require",
    ])

    DataProtection__BlobUri     = "${local.dataprotection_container.url}/keys.xml"
    DataProtection__KeyVaultKey = module.key_vault.keys["dataprotection"].versionless_id

    APPLICATIONINSIGHTS_CONNECTION_STRING = module.application_insights.connection_string
  }
}

# --- What the web app's managed identity may do ---

resource "azurerm_role_assignment" "web_app_acr_pull" {
  scope                = var.container_registry_id
  role_definition_name = "AcrPull"
  principal_id         = module.web_app.principal_id
}

# Certificates are read with their private key through their secret.
resource "azurerm_role_assignment" "web_app_key_vault_secrets" {
  scope                = module.key_vault.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = module.web_app.principal_id
}

resource "azurerm_role_assignment" "web_app_dataprotection_key" {
  scope                = module.key_vault.keys["dataprotection"].resource_versionless_id
  role_definition_name = "Key Vault Crypto User"
  principal_id         = module.web_app.principal_id
}

resource "azurerm_role_assignment" "web_app_dataprotection_blobs" {
  scope                = local.dataprotection_container.scope
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = module.web_app.principal_id
}
