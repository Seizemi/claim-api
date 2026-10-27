# Infrastructure

Terraform for the Azure resources that host ClaimApi and for the GitHub settings its pipeline uses. Region: `francecentral`. No Azure identifier (subscription, tenant, state storage) is written in this folder: Terraform reads them from [environment variables](#environment-variables).

| Folder | Content | State |
|---|---|---|
| `modules/<resource>/` | One module per Azure resource, with the sub-resources that can't exist without it (see below) | |
| `modules/app-environment/` | One application environment: names and wires the resource modules, grants the roles between them | |
| `azure/shared/` | Resources used by every environment: Container Registry, Log Analytics | `shared.tfstate` |
| `azure/environment/` | Every application environment, one Terraform workspace each (`staging`, later `prod`), set by `tfvars/<workspace>.tfvars` | `environment.tfstateenv:<workspace>` |
| `github/` | GitHub environments and variables of the `claim-api` repository, from the Azure outputs | `github.tfstate` |

Naming: `<type>-claim-<env>-frc` (for example `app-claim-staging-frc`). Globally unique names that can't contain hyphens (registry, storage accounts) get a short suffix.

## Prerequisites

- Terraform 1.15 and Azure CLI 2.87 or later.
- **Owner** on the subscription (or Contributor + User Access Administrator): Terraform creates role assignments.

## Terraform state storage (once per subscription)

Terraform can't keep its state in a storage account it creates itself, so this one is created by hand, outside Terraform: resource group `rg-claim-tfstate-frc` (with a `CanNotDelete` lock) holding the storage account `stclaimtfstate<suffix>` and its private `tfstate` container. Only Azure AD sign-in works on it (no access keys), blob versioning and 30-day soft delete keep every previous state, and you need `Storage Blob Data Contributor` on it. The identity the workflows run Terraform with is created by hand too (see [From GitHub Actions](#from-github-actions)). Their names and IDs are the values of the [environment variables](#environment-variables) and of the GitHub environment `infrastructure`.

**Restoring a previous state:** Azure portal > storage account > *Containers* > `tfstate` > the `.tfstate` blob > *Versions*, then promote the version to restore.

## Modules

Each resource module has `main.tf`, `variables.tf` and `outputs.tf`. Its variables are what a caller can set: `name`, `location`, `resource_group_name` and `tags` everywhere, then the resource's own settings (SKU, version, retention...), with defaults where one makes sense. Choices that are part of the project's security baseline stay fixed in `main.tf` (HTTPS/TLS 1.2 only, no access keys or admin users, Entra-only PostgreSQL, RBAC Key Vault). Role assignments between resources are made by the caller, not by the modules.

| Module | Contains |
|---|---|
| `resource-group` | Resource group |
| `container-registry` | Container Registry |
| `log-analytics-workspace` | Log Analytics workspace |
| `virtual-network` | VNet and its subnets (optionally delegated) |
| `network-security-group` | NSG, its rules and its subnet associations |
| `private-dns-zone` | Private DNS zone and its VNet links |
| `postgresql-flexible-server` | Server (Entra only), databases, Entra administrators (created one at a time) |
| `key-vault` | Vault (RBAC), administrator role for the deployer, self-signed certificates, keys |
| `storage-account` | Storage account (no access keys) and private containers |
| `app-service-plan` | App Service plan |
| `linux-web-app` | Container web app with a system-assigned identity (the image tag is left to deployments) |
| `application-insights` | Application Insights (workspace-based) |
| `diagnostic-setting` | Diagnostic setting sending logs and metrics to Log Analytics |
| `user-assigned-identity` | Managed identity and its federated credentials |

## What an environment contains (`modules/app-environment`)

| Resource | Name (staging) | Notes |
|---|---|---|
| Resource group | `rg-claim-staging-frc` | |
| VNet + subnets | `vnet-claim-staging-frc`: `snet-app` (App Service VNet integration), `snet-postgres` (delegated) | NSG: PostgreSQL accepts 5432 from `snet-app` only. Only private traffic goes through the VNet. |
| PostgreSQL Flexible Server | `psql-claim-staging-frc`, database `claimapi` | Private IP only (private DNS zone). **Entra authentication only, no password**: the web app's managed identity and the person running Terraform are the Entra administrators. |
| Key Vault | `kv-claim-staging-frc` | RBAC. Self-signed certificates `partner-portal-web` and `partner-portal-graph` (12 months, auto-renewed) and the `dataprotection` key. Private keys never leave the vault. |
| Storage account | `stclaimstaging<suffix>`, container `dataprotection` | Data Protection key ring. No access keys. |
| App Service plan + web app | `asp-claim-staging-frc` (B1), `app-claim-staging-frc` | Container from the registry, pulled with the system-assigned managed identity. HTTPS only, health check `/healthz`. |
| Application Insights | `appi-claim-staging-frc` | In the shared Log Analytics workspace, like the web app and PostgreSQL logs. |
| GitHub Actions identity | `id-claim-staging-frc-github` | Federated credential for `repo:Seizemi/claim-api:environment:staging`. `AcrPush` on the registry, `Website Contributor` on the web app. |

The web app's identity has `AcrPull` on the registry, `Key Vault Secrets User` on the vault, `Key Vault Crypto User` on the `dataprotection` key and `Storage Blob Data Contributor` on the `dataprotection` container.

## Environment variables

The backends are partial configurations and the roots have no subscription variable, so every Terraform command needs these variables ([Terraform state storage](#terraform-state-storage-once-per-subscription)). The workflows set them from the variables of the GitHub environment `infrastructure`; locally, set them in the PowerShell window before running Terraform:

```powershell
$env:ARM_SUBSCRIPTION_ID = '<subscription-id>'                     # azurerm provider and backend
$env:TF_VAR_tfstate_resource_group_name = 'rg-claim-tfstate-frc'   # terraform_remote_state
$env:TF_VAR_tfstate_storage_account_name = 'stclaimtfstate<suffix>'
# terraform init of every root then finds the state storage:
$env:TF_CLI_ARGS_init = "-backend-config=resource_group_name=$env:TF_VAR_tfstate_resource_group_name -backend-config=storage_account_name=$env:TF_VAR_tfstate_storage_account_name"
```

| Variable | Read by | GitHub variable (environment `infrastructure`) |
|---|---|---|
| `ARM_SUBSCRIPTION_ID` | azurerm provider, backend, `terraform_remote_state` | `AZURE_SUBSCRIPTION_ID` |
| `TF_VAR_tfstate_resource_group_name` | `terraform_remote_state` (`azure/environment`, `github`), and `-backend-config` | `TFSTATE_RESOURCE_GROUP` |
| `TF_VAR_tfstate_storage_account_name` | same | `TFSTATE_STORAGE_ACCOUNT` |
| `TF_CLI_ARGS_init` | `terraform init` (`-backend-config`) | built by the workflows from the two above |

A root that doesn't declare a `TF_VAR_` variable ignores it. A folder initialized before the backends became partial asks to migrate the state on the next `init`: run `terraform init -reconfigure` once (same state, nothing is moved).

## Applying

Order: `azure/shared`, then `azure/environment` for each environment, then `github` (each reads the previous outputs).

**`azure/shared` and `github`**, from their folder:

```powershell
terraform init
terraform plan -out main.tfplan
terraform apply main.tfplan
```

**`azure/environment`**: one root for every environment. The Terraform **workspace** is the environment: it picks the state (blob `environment.tfstateenv:<workspace>`) and names the resources (`app-claim-<workspace>-frc`...). Each environment has two variable files:

| File | Content | Git |
|---|---|---|
| `tfvars/<workspace>.tfvars` | `environment` (must equal the workspace), SKUs, PostgreSQL storage, Key Vault retention | committed |
| `tfvars/<workspace>.entra.tfvars` | `entra`: the External ID tenant values (copy `tfvars/<workspace>.entra.tfvars.example`; staging uses the **dev** tenant) | ignored |

```powershell
terraform init
terraform workspace select -or-create staging
terraform plan "-var-file=tfvars/staging.tfvars" "-var-file=tfvars/staging.entra.tfvars" -out main.tfplan
terraform apply main.tfplan
```

The plan stops before anything else if the `default` workspace is selected, or if the variable files are for another environment than the workspace. **Adding an environment** (prod): create `tfvars/prod.tfvars` and `tfvars/prod.entra.tfvars.example`, then run the commands above with `prod`, then add `prod` to `environments` in `github/variables.tf`, and an `azure/environment (prod)` step to both workflows with the secret `ENTRA_PROD` ([From GitHub Actions](#from-github-actions)).

- **`github`** needs a GitHub token in the `GITHUB_TOKEN` environment variable (current window only): a **fine-grained** token on `Seizemi/claim-api` with *Administration*, *Environments* and *Variables* read and write. For each environment of `environments` it writes the GitHub environment of the same name (deployments from `main` only) and its variables `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `ACR_NAME`, `ACR_LOGIN_SERVER`, `RESOURCE_GROUP`, `WEBAPP_NAME`, `PUBLIC_URL`. No secret: the pipeline signs in to Azure with OpenID Connect.

**Never put a secret value in Terraform:** the state stores every value in plain text. Secrets are generated in Key Vault or replaced by managed identities.

Before the first deployment, the web app points to `claimapi:latest`, which doesn't exist yet: it stays unhealthy until an image is pushed. Deployments change the image tag; Terraform ignores it.

## From GitHub Actions

Two workflows of this repository, run by hand only (*Actions* > the workflow > *Run workflow*, on `main`):

| Workflow | Does |
|---|---|
| `infra-apply` | `plan` then `apply` of `azure/shared`, `azure/environment` (workspace `staging`), `github`, in that order |
| `infra-destroy` | `destroy` of `github`, `azure/environment` (workspace `staging`), `azure/shared`, in that order. Asks you to type `destroy`. The state storage and the Terraform identity stay. |

They never run at the same time (same concurrency group). They sign in to Azure as **`id-claim-terraform-frc`**, a user-assigned managed identity in `rg-claim-tfstate-frc`, created by hand (a pipeline can't sign in with an identity its own Terraform creates or destroys), through OpenID Connect: no Azure secret is stored in GitHub, and only jobs of the GitHub environment `infrastructure` of `Seizemi/claim-api` get a token. Its rights:

- `Contributor` on the subscription;
- `Role Based Access Control Administrator` on the subscription, **limited by a condition** to assigning and removing the roles this Terraform uses (`AcrPull`, `AcrPush`, `Website Contributor`, `Key Vault Administrator`, `Key Vault Secrets User`, `Key Vault Crypto User`, `Storage Blob Data Contributor`: every `role_definition_name` of this Terraform, keep the condition in sync);
- `Storage Blob Data Contributor` on the state storage account.

It has no Entra (directory) right: the workflows pass its name in `TF_VAR_deployer_principal_name` instead of looking it up. Like a person running Terraform locally, it becomes the Key Vault administrator and a PostgreSQL Entra administrator of the environment.

**Setup, once** (repository *Settings* > *Environments* > *New environment* `infrastructure`):

1. Create the identity `id-claim-terraform-frc` in `rg-claim-tfstate-frc`, its federated credential (issuer `https://token.actions.githubusercontent.com`, subject `repo:Seizemi/claim-api:environment:infrastructure`, audience `api://AzureADTokenExchange`) and the three role assignments above (subscription or state storage account > *Access control (IAM)*). Note its client ID, the tenant ID, the subscription ID and the state storage names.
2. *Deployment branches and tags*: **Selected branches**, `main`. Optionally, *Required reviewers*: yourself.
3. *Environment variables*, from step 1: `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `TFSTATE_RESOURCE_GROUP`, `TFSTATE_STORAGE_ACCOUNT`. Variables, not secrets: they're identifiers, useless without the federated sign-in (they would be masked as `***` in every log otherwise).
4. *Environment secrets*:
   - `ENTRA_STAGING`: the value of `entra` from `tfvars/staging.entra.tfvars`, the braces included (`{ authority = "...", tenant_id = "...", ... }`, on one or several lines). It reaches Terraform as `TF_VAR_entra`. A secret, not a variable: the repository and its Actions logs are public.
   - `TERRAFORM_GITHUB_TOKEN`: a **fine-grained** token on `Seizemi/claim-api` with *Administration*, *Environments* and *Variables* read and write (the one `github` uses locally). Renew it before it expires.

A `workflow_dispatch` workflow only shows up in *Actions* once its file is on the default branch.

**Local runs and workflow runs on the same state:** the deployer (Key Vault administrator, PostgreSQL administrator) is whoever runs Terraform, so a local `apply` after a workflow `apply` replaces the identity by you, and the next workflow run swaps it back. Once the workflows are in use, apply from them only, and keep local runs to `plan`.

The plan file and the state hold the tenant values in clear text: the workflows never upload a plan as an artifact and never print the state. Keep it that way.

The GitHub Actions identity of each environment (`id-claim-<env>-frc-github`, above) is a different one: it only deploys the application (push the image, update the web app).
