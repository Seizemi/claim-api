# The public part of the two Key Vault certificates (keyvault.tf), uploaded to their app registrations in the External
# ID tenant: Entra accepts the app's client assertions only when signed with a registered certificate. Destroying the
# environment removes them again, so the registrations don't collect the certificates of every rebuilt Key Vault.
#
# Renewal is NOT handled: Key Vault renews a certificate 30 days before it expires and the app uses the new version
# within 12 hours, before anything uploads it here. Harmless while environments are rebuilt well within the 12-month
# validity; to solve before production (spec 10.6).

locals {
  app_registration_client_ids = {
    web   = var.entra.web_client_id
    graph = var.entra.graph_client_id
  }
}

data "azuread_application" "external" {
  provider = azuread.external
  for_each = toset(["web", "graph"])

  client_id = local.app_registration_client_ids[each.key]
}

resource "azuread_application_certificate" "external" {
  provider = azuread.external
  for_each = toset(["web", "graph"])

  application_id = "/applications/${data.azuread_application.external[each.key].object_id}"
  type           = "AsymmetricX509Cert"
  encoding       = "hex"
  value          = module.key_vault.certificates[each.key].data_hex
  start_date     = module.key_vault.certificates[each.key].not_before
  end_date       = module.key_vault.certificates[each.key].expires
}
