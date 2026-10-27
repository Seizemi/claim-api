# Container on App Service for Linux. The system-assigned managed identity pulls the image from the registry.

resource "azurerm_linux_web_app" "this" {
  name                                           = var.name
  resource_group_name                            = var.resource_group_name
  location                                       = var.location
  service_plan_id                                = var.service_plan_id
  https_only                                     = true
  client_affinity_enabled                        = var.client_affinity_enabled
  ftp_publish_basic_authentication_enabled       = false
  webdeploy_publish_basic_authentication_enabled = false
  virtual_network_subnet_id                      = var.virtual_network_subnet_id
  app_settings                                   = var.app_settings
  tags                                           = var.tags

  identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on                               = var.always_on
    minimum_tls_version                     = "1.2"
    ftps_state                              = "Disabled"
    health_check_path                       = var.health_check_path
    health_check_eviction_time_in_min       = var.health_check_eviction_time_in_min
    container_registry_use_managed_identity = true
    vnet_route_all_enabled                  = var.vnet_route_all_enabled

    application_stack {
      docker_registry_url = "https://${var.container_registry_login_server}"
      docker_image_name   = var.docker_image_name
    }
  }

  lifecycle {
    # Deployments (GitHub Actions) change the image tag; Terraform must not roll it back.
    ignore_changes = [site_config[0].application_stack[0].docker_image_name]
  }
}
