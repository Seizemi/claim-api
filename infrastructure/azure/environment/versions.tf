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
  features {}
  storage_use_azuread = true
}

# The Azure (workforce) tenant of the subscription, signed in with the Azure CLI. Never the External ID tenant.
provider "azuread" {}
