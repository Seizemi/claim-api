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
