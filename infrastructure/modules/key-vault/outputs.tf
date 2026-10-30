output "id" {
  value = azurerm_key_vault.this.id
}

output "name" {
  value = azurerm_key_vault.this.name
}

output "vault_uri" {
  value = azurerm_key_vault.this.vault_uri
}

output "certificate_names" {
  description = "Certificate names by the keys of var.certificates."
  value       = { for key, certificate in azurerm_key_vault_certificate.this : key => certificate.name }
}

output "certificates" {
  description = "By the keys of var.certificates: the public certificate (DER, hex) of the current version and its validity dates (RFC 3339)."
  value = {
    for key, certificate in azurerm_key_vault_certificate.this : key => {
      name       = certificate.name
      data_hex   = certificate.certificate_data
      not_before = certificate.certificate_attribute[0].not_before
      expires    = certificate.certificate_attribute[0].expires
    }
  }
}

output "keys" {
  description = "By the keys of var.keys: versionless_id (data plane URI, what apps use) and resource_versionless_id (role assignment scope)."
  value = {
    for key, vault_key in azurerm_key_vault_key.this : key => {
      name                    = vault_key.name
      versionless_id          = vault_key.versionless_id
      resource_versionless_id = vault_key.resource_versionless_id
    }
  }
}
