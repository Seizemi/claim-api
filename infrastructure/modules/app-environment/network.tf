# The web app reaches PostgreSQL over the VNet; the server has no public endpoint.
# Key Vault and the Data Protection storage keep public endpoints (Azure AD sign-in and RBAC only).

module "virtual_network" {
  source = "../virtual-network"

  name                = "vnet-${local.name}"
  resource_group_name = module.resource_group.name
  location            = var.location
  address_space       = [var.vnet_address_space]
  tags                = local.tags

  subnets = {
    # App Service VNet integration. Only private (RFC 1918) traffic goes through it: Entra, Graph and the registry are
    # reached directly, so the subnet needs no NAT gateway. It carries outbound traffic only, so no NSG is attached.
    snet-app = {
      address_prefixes = [var.app_subnet_prefix]
      delegation = {
        name    = "app-service"
        service = "Microsoft.Web/serverFarms"
        actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
      }
    }

    snet-postgres = {
      address_prefixes = [var.postgres_subnet_prefix]
      delegation = {
        name    = "postgres"
        service = "Microsoft.DBforPostgreSQL/flexibleServers"
        actions = ["Microsoft.Network/virtualNetworks/subnets/join/action"]
      }
    }
  }
}

# PostgreSQL accepts connections from the app subnet only.
module "postgres_network_security_group" {
  source = "../network-security-group"

  name                = "nsg-${local.name}-postgres"
  resource_group_name = module.resource_group.name
  location            = var.location
  tags                = local.tags

  subnet_ids = {
    postgres = module.virtual_network.subnet_ids["snet-postgres"]
  }

  security_rules = {
    allow-postgres-from-app = {
      priority                   = 100
      direction                  = "Inbound"
      access                     = "Allow"
      protocol                   = "Tcp"
      source_address_prefix      = var.app_subnet_prefix
      destination_address_prefix = var.postgres_subnet_prefix
      destination_port_range     = "5432"
    }

    # Traffic inside the subnet (server maintenance, high availability if enabled later).
    allow-within-subnet = {
      priority                   = 110
      direction                  = "Inbound"
      access                     = "Allow"
      source_address_prefix      = var.postgres_subnet_prefix
      destination_address_prefix = var.postgres_subnet_prefix
    }

    # Overrides the default rule that lets the whole VNet in.
    deny-other-vnet-inbound = {
      priority              = 4000
      direction             = "Inbound"
      access                = "Deny"
      source_address_prefix = "VirtualNetwork"
    }
  }
}

# Resolves the server name to its private IP for everything in the VNet, the web app included.
module "postgres_private_dns_zone" {
  source = "../private-dns-zone"

  name                = "${local.name}.private.postgres.database.azure.com"
  resource_group_name = module.resource_group.name
  tags                = local.tags

  virtual_network_links = {
    "link-${local.name}" = module.virtual_network.id
  }
}