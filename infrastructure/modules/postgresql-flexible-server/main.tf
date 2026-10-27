# Flexible server injected into a delegated subnet (private IP only), with Entra authentication only: no password
# exists. The private DNS zone must be linked to the VNet before the server is created.

resource "azurerm_postgresql_flexible_server" "this" {
  name                          = var.name
  resource_group_name           = var.resource_group_name
  location                      = var.location
  version                       = var.postgres_version
  sku_name                      = var.sku_name
  storage_mb                    = var.storage_mb
  backup_retention_days         = var.backup_retention_days
  delegated_subnet_id           = var.delegated_subnet_id
  private_dns_zone_id           = var.private_dns_zone_id
  public_network_access_enabled = false
  tags                          = var.tags

  authentication {
    active_directory_auth_enabled = true
    password_auth_enabled         = false
    tenant_id                     = var.tenant_id
  }

  # The zone isn't set: Azure picks one. With high availability, a failover moves the server to the standby's zone;
  # without this, the next apply would fail it back to the original zone (another outage). Burstable SKUs have no high
  # availability, so this only matters for a General Purpose server, which should also ignore
  # high_availability[0].standby_availability_zone.
  lifecycle {
    ignore_changes = [zone]
  }
}

resource "azurerm_postgresql_flexible_server_database" "this" {
  for_each  = toset(var.databases)
  name      = each.value
  server_id = azurerm_postgresql_flexible_server.this.id
  charset   = var.database_charset
  collation = var.database_collation
}

# Entra administrators are added one at a time: the first one, then the others once it exists.
locals {
  first_administrator = var.administrators[0]
  other_administrators = {
    for administrator in slice(var.administrators, 1, length(var.administrators)) :
    administrator.principal_name => administrator
  }
}

resource "azurerm_postgresql_flexible_server_active_directory_administrator" "first" {
  server_name         = azurerm_postgresql_flexible_server.this.name
  resource_group_name = var.resource_group_name
  tenant_id           = var.tenant_id
  object_id           = local.first_administrator.object_id
  principal_name      = local.first_administrator.principal_name
  principal_type      = local.first_administrator.principal_type
}

resource "azurerm_postgresql_flexible_server_active_directory_administrator" "others" {
  for_each            = local.other_administrators
  server_name         = azurerm_postgresql_flexible_server.this.name
  resource_group_name = var.resource_group_name
  tenant_id           = var.tenant_id
  object_id           = each.value.object_id
  principal_name      = each.value.principal_name
  principal_type      = each.value.principal_type

  depends_on = [azurerm_postgresql_flexible_server_active_directory_administrator.first]
}
