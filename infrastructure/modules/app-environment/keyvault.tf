# Certificates the app authenticates to Entra External ID with (sign-in and Graph), and the key that encrypts the
# Data Protection key ring. The web app reads them with its managed identity.

module "key_vault" {
  source = "../key-vault"

  name                       = "kv-${local.name}"
  resource_group_name        = module.resource_group.name
  location                   = var.location
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  purge_protection_enabled   = var.key_vault_purge_protection
  soft_delete_retention_days = var.key_vault_soft_delete_days
  administrator_principal_id = data.azurerm_client_config.current.object_id
  tags                       = local.tags

  # Self-signed, renewed by Key Vault 30 days before expiry. Entra trusts a certificate only once its public part is
  # uploaded to the app registration: after each renewal, upload the new one (both can be registered during the overlap).
  certificates = {
    web = {
      name    = "partner-portal-web"
      subject = "CN=claim-${var.environment}-partner-portal-web"
    }
    graph = {
      name    = "partner-portal-graph"
      subject = "CN=claim-${var.environment}-partner-portal-graph"
    }
  }

  # Wraps the Data Protection keys stored in blob storage (storage.tf).
  keys = {
    dataprotection = {
      name     = "dataprotection"
      key_opts = ["wrapKey", "unwrapKey"]
    }
  }
}