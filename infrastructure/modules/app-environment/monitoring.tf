# Application Insights (traces, requests, exceptions) and the platform logs of the web app and PostgreSQL, all in the
# shared Log Analytics workspace.

module "application_insights" {
  source = "../application-insights"

  name                       = "appi-${local.name}"
  resource_group_name        = module.resource_group.name
  location                   = var.location
  log_analytics_workspace_id = var.log_analytics_workspace_id
  tags                       = local.tags
}

module "web_app_diagnostics" {
  source = "../diagnostic-setting"

  target_resource_id         = module.web_app.id
  log_analytics_workspace_id = var.log_analytics_workspace_id
}

module "postgres_diagnostics" {
  source = "../diagnostic-setting"

  target_resource_id         = module.postgres.id
  log_analytics_workspace_id = var.log_analytics_workspace_id
}
