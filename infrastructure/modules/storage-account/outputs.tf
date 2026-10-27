output "id" {
  value = azurerm_storage_account.this.id
}

output "name" {
  value = azurerm_storage_account.this.name
}

output "primary_blob_endpoint" {
  value = azurerm_storage_account.this.primary_blob_endpoint
}

output "containers" {
  description = "By name: url (data plane) and scope (resource ID, for role assignments)."
  value = {
    for name, container in azurerm_storage_container.this : name => {
      url   = "${azurerm_storage_account.this.primary_blob_endpoint}${container.name}"
      scope = "${azurerm_storage_account.this.id}/blobServices/default/containers/${container.name}"
    }
  }
}
