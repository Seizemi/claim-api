terraform {
  required_version = "~> 1.15"

  required_providers {
    github = {
      source  = "integrations/github"
      version = "~> 6.13"
    }
  }

  backend "azurerm" {
    container_name   = "tfstate"
    key              = "github.tfstate"
    use_azuread_auth = true
  }
}

# Authenticates with the GITHUB_TOKEN environment variable of your machine (see README): a fine-grained token on
# Seizemi/claim-api with Administration, Environments and Variables read/write. Nothing else reads it.
provider "github" {
  owner = var.github_owner
}
