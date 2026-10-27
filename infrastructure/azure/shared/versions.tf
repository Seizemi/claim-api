terraform {
  required_version = "~> 1.15"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 5.9"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.9"
    }
  }

  backend "azurerm" {
    container_name   = "tfstate"
    key              = "shared.tfstate"
    use_azuread_auth = true
  }
}

# Subscription from ARM_SUBSCRIPTION_ID.
provider "azurerm" {
  features {}
  storage_use_azuread = true
}
