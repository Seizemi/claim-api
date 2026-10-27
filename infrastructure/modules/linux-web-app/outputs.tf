output "id" {
  value = azurerm_linux_web_app.this.id
}

output "name" {
  value = azurerm_linux_web_app.this.name
}

output "default_hostname" {
  value = azurerm_linux_web_app.this.default_hostname
}

output "principal_id" {
  description = "Object ID of the system-assigned managed identity."
  value       = azurerm_linux_web_app.this.identity[0].principal_id
}
