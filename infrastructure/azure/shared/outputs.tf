# Read by azure/<environment> and github/ through terraform_remote_state.

output "container_registry_id" {
  value = module.container_registry.id
}

output "container_registry_name" {
  value = module.container_registry.name
}

output "container_registry_login_server" {
  value = module.container_registry.login_server
}

output "log_analytics_workspace_id" {
  value = module.log_analytics_workspace.id
}
