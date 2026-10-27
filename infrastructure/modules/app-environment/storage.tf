# Data Protection key ring (the keys that encrypt the session cookie), shared by every instance and kept across
# restarts and deployments. Each key is encrypted with the Key Vault key (keyvault.tf).

module "dataprotection_storage" {
  source = "../storage-account"

  name                = "stclaim${var.environment}${random_string.suffix.result}"
  resource_group_name = module.resource_group.name
  location            = var.location
  containers          = ["dataprotection"]
  tags                = local.tags
}

locals {
  dataprotection_container = module.dataprotection_storage.containers["dataprotection"]
}
