# terraform workspace select staging
# terraform plan "-var-file=tfvars/staging.tfvars" "-var-file=tfvars/staging.entra.tfvars" -out main.tfplan
environment = "staging"

app_service_sku = "B1" # no slots: deployments go straight to the app

postgres_sku        = "B_Standard_B1ms"
postgres_storage_mb = 32768

# Staging can be torn down and rebuilt: a deleted vault doesn't block its name for 90 days.
key_vault_purge_protection = false
key_vault_soft_delete_days = 7
