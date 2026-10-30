terraform {
  required_version = "~> 1.15"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 5.9"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 3.10"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.9"
    }
    time = {
      source  = "hashicorp/time"
      version = "~> 0.14"
    }
  }

  # Created by hand (README, "Terraform state storage"). Partial configuration, no Azure identifier in git: the
  # subscription comes from ARM_SUBSCRIPTION_ID, the resource group and storage account from terraform init
  # -backend-config (README, "Environment variables").
  backend "azurerm" {
    container_name   = "tfstate"
    key              = "environment.tfstate" # one blob per workspace: environment.tfstateenv:<workspace>
    use_azuread_auth = true
  }
}

# Subscription from ARM_SUBSCRIPTION_ID.
provider "azurerm" {
  features {
    # Azure creates the "Application Insights Smart Detection" action group outside Terraform; it would block the destroy.
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
  storage_use_azuread = true
}

# The Azure (workforce) tenant of the subscription, signed in with the Azure CLI. Never the External ID tenant.
provider "azuread" {}

# The environment's External ID tenant: uploads the Key Vault certificates to its app registrations
# (modules/app-environment/entra.tf). GitHub Actions: its claim-terraform app registration through OpenID Connect.
# Locally: the Azure CLI, signed in to that tenant too (README, "External ID tenant").
provider "azuread" {
  alias     = "external"
  tenant_id = var.entra.tenant_id
  client_id = var.entra.terraform_client_id
  use_oidc  = var.entra.terraform_client_id != null
  use_cli   = var.entra.terraform_client_id == null
}
