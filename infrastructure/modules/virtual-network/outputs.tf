output "id" {
  value = azurerm_virtual_network.this.id
}

output "name" {
  value = azurerm_virtual_network.this.name
}

output "subnet_ids" {
  description = "Subnet IDs by name."
  value       = { for name, subnet in azurerm_subnet.this : name => subnet.id }
}
