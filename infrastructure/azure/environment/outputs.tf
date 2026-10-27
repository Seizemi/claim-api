# Read by github/ through terraform_remote_state (one state per workspace), and printed after apply for step 3.3.

output "resource_group_name" {
  value = module.environment.resource_group_name
}

output "web_app_name" {
  value = module.environment.web_app_name
}

output "public_url" {
  value = module.environment.public_url
}

output "key_vault_name" {
  value = module.environment.key_vault_name
}

output "certificate_names" {
  value = module.environment.certificate_names
}

output "postgres_fqdn" {
  value = module.environment.postgres_fqdn
}

output "tenant_id" {
  value = module.environment.tenant_id
}

output "subscription_id" {
  value = module.environment.subscription_id
}

output "github_client_id" {
  value = module.environment.github_client_id
}
