output "resource_group_name" {
  value = module.resource_group.name
}

output "web_app_name" {
  value = module.web_app.name
}

output "public_url" {
  description = "Register <public_url>/signin-oidc and <public_url>/signout-callback-oidc in partner-portal-web (step 3.3)."
  value       = local.public_url
}

output "key_vault_name" {
  value = module.key_vault.name
}

output "certificate_names" {
  description = "Upload the public part of each to its app registration (step 3.3)."
  value       = module.key_vault.certificate_names
}

output "postgres_fqdn" {
  value = module.postgres.fqdn
}

output "tenant_id" {
  description = "Azure (workforce) tenant of the subscription, not the External ID tenant."
  value       = data.azurerm_client_config.current.tenant_id
}

output "subscription_id" {
  value = data.azurerm_client_config.current.subscription_id
}

output "github_client_id" {
  description = "Client ID GitHub Actions signs in with (AZURE_CLIENT_ID)."
  value       = module.github_identity.client_id
}
