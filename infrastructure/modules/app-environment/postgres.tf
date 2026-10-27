# Private server (VNet only) with Entra authentication only: no password exists. Two Entra administrators:
# the web app's managed identity (it runs the EF migrations at startup, so it needs DDL rights) and the person running
# Terraform (administration, from inside the VNet).

module "postgres" {
  source = "../postgresql-flexible-server"

  name                = "psql-${local.name}"
  resource_group_name = module.resource_group.name
  location            = var.location
  postgres_version    = var.postgres_version
  sku_name            = var.postgres_sku
  storage_mb          = var.postgres_storage_mb
  delegated_subnet_id = module.virtual_network.subnet_ids["snet-postgres"]
  private_dns_zone_id = module.postgres_private_dns_zone.id
  tenant_id           = data.azurerm_client_config.current.tenant_id
  databases           = [var.postgres_database]
  tags                = local.tags

  administrators = [
    # The application connects as this role: its name is the web app's name (connection string username).
    {
      object_id      = module.web_app.principal_id
      principal_name = local.web_app_name
      principal_type = "ServicePrincipal"
    },
    local.deployer,
  ]

  # The server needs the VNet link of its DNS zone, and its subnet's NSG, when it is created.
  depends_on = [
    module.postgres_private_dns_zone,
    module.postgres_network_security_group,
  ]
}
