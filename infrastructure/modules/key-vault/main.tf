# RBAC vault with certificates and keys generated inside it: private keys never leave it and never reach the
# Terraform state.

terraform {
  required_providers {
    time = {
      source = "hashicorp/time"
    }
  }
}

resource "azurerm_key_vault" "this" {
  name                          = var.name
  resource_group_name           = var.resource_group_name
  location                      = var.location
  tenant_id                     = var.tenant_id
  sku_name                      = var.sku_name
  rbac_authorization_enabled    = true
  purge_protection_enabled      = var.purge_protection_enabled
  soft_delete_retention_days    = var.soft_delete_retention_days
  public_network_access_enabled = var.public_network_access_enabled
  tags                          = var.tags
}

# Lets the principal running Terraform create the certificates and keys below.
resource "azurerm_role_assignment" "administrator" {
  scope                = azurerm_key_vault.this.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = var.administrator_principal_id
}

# A new role assignment takes a while to apply on the vault's data plane.
resource "time_sleep" "rbac_propagation" {
  depends_on      = [azurerm_role_assignment.administrator]
  create_duration = var.rbac_propagation_wait
}

# Self-signed, renewed by Key Vault before expiry.
resource "azurerm_key_vault_certificate" "this" {
  for_each     = var.certificates
  name         = each.value.name
  key_vault_id = azurerm_key_vault.this.id
  tags         = var.tags

  certificate_policy {
    issuer_parameters {
      name = "Self"
    }

    key_properties {
      exportable = each.value.exportable
      key_type   = "RSA"
      key_size   = each.value.key_size
      reuse_key  = false
    }

    secret_properties {
      content_type = "application/x-pkcs12"
    }

    x509_certificate_properties {
      subject            = each.value.subject
      validity_in_months = each.value.validity_in_months
      key_usage          = ["digitalSignature", "keyEncipherment"]
    }

    lifetime_action {
      action {
        action_type = "AutoRenew"
      }
      trigger {
        days_before_expiry = each.value.renew_days_before_expiry
      }
    }
  }

  depends_on = [time_sleep.rbac_propagation]
}

resource "azurerm_key_vault_key" "this" {
  for_each     = var.keys
  name         = each.value.name
  key_vault_id = azurerm_key_vault.this.id
  key_type     = each.value.key_type
  key_size     = each.value.key_size
  key_opts     = each.value.key_opts
  tags         = var.tags

  depends_on = [time_sleep.rbac_propagation]
}
