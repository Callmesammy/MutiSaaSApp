# 🚀 DEPLOY TO AZURE - COMPLETE GUIDE

## **✅ Build Status**
- Build: **SUCCESS**
- Tests: **41/41 PASSING**
- Ready to Deploy: **YES**

---

## **🎯 Azure Deployment Options**

Choose one based on your needs:

### **Option 1: Azure App Service (Easiest) ⭐ RECOMMENDED**
- Best for: Simple deployment, automatic scaling, HTTPS
- Cost: ~$50-500/month depending on tier
- Setup time: 15 minutes

### **Option 2: Azure Container Instances (Fastest)**
- Best for: Quick testing, serverless containers
- Cost: ~$10-50/month
- Setup time: 10 minutes

### **Option 3: Azure Kubernetes Service (Most Powerful)**
- Best for: Enterprise, complex workloads, auto-scaling
- Cost: ~$100+/month
- Setup time: 30 minutes

### **Option 4: GitHub Actions → Azure (Fully Automated)**
- Best for: CI/CD pipeline, automatic deployments on push
- Cost: Variable (pay as you go)
- Setup time: 20 minutes

---

## **📋 PREREQUISITES**

Before you start, you need:

```bash
# 1. Install Azure CLI
# Download from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli

# 2. Login to Azure
az login

# 3. Check your subscriptions
az account list --output table

# 4. Set default subscription
az account set --subscription "YOUR_SUBSCRIPTION_ID"
```

---

## **🚀 OPTION 1: AZURE APP SERVICE (RECOMMENDED)**

### **Step 1: Create Resource Group**

```bash
az group create \
  --name teamflow-rg \
  --location eastus
```

### **Step 2: Create App Service Plan**

```bash
az appservice plan create \
  --name teamflow-plan \
  --resource-group teamflow-rg \
  --sku B2 \
  --is-linux
```

**SKU Options:**
- `B1` - $10/month (basic)
- `B2` - $50/month (recommended for dev/test)
- `B3` - $100/month (better performance)
- `S1` - $50/month (standard)
- `P1V2` - $100+/month (premium)

### **Step 3: Create Web App**

```bash
az webapp create \
  --resource-group teamflow-rg \
  --plan teamflow-plan \
  --name teamflow-api \
  --deployment-container-image-name-prefix teamflow
```

**Note:** App name must be globally unique. Use something like `teamflow-api-{your-username}`

### **Step 4: Create SQL Database**

```bash
# Create SQL Server
az sql server create \
  --name teamflow-sqlserver \
  --resource-group teamflow-rg \
  --location eastus \
  --admin-user adminuser \
  --admin-password "YourP@ssw0rd123!"

# Create Database
az sql db create \
  --resource-group teamflow-rg \
  --server teamflow-sqlserver \
  --name TeamFlowDb \
  --service-objective S1

# Get Connection String
az sql db show-connection-string \
  --client ado.net \
  --server teamflow-sqlserver \
  --name TeamFlowDb
```

### **Step 5: Create Azure Redis Cache (Optional)**

```bash
az redis create \
  --name teamflow-redis \
  --resource-group teamflow-rg \
  --location eastus \
  --sku Basic \
  --vm-size c0
```

### **Step 6: Configure App Settings**

```bash
az webapp config appsettings set \
  --resource-group teamflow-rg \
  --name teamflow-api \
  --settings \
    "ConnectionStrings:DefaultConnection=YOUR_SQL_CONNECTION_STRING" \
    "Jwt:Secret=your-super-secret-key-change-this" \
    "Jwt:ExpiryMinutes=60" \
    "Redis:Enabled=true" \
    "Redis:ConnectionString=your-redis-connection-string"
```

### **Step 7: Deploy from Git**

```bash
# Link GitHub Repository
az webapp deployment github-actions add \
  --resource-group teamflow-rg \
  --name teamflow-api \
  --repo https://github.com/Callmesammy/MutiSaaSApp \
  --branch master \
  --runtime dotnet \
  --runtime-version 10.0
```

Or deploy manually:

```bash
# Build and publish
dotnet publish -c Release -o ./publish

# Create zip
cd publish
zip -r ../app.zip .
cd ..

# Deploy zip file
az webapp up \
  --resource-group teamflow-rg \
  --name teamflow-api \
  --package ./app.zip
```

---

## **🐳 OPTION 2: AZURE CONTAINER INSTANCES**

### **Step 1: Create Container Registry**

```bash
az acr create \
  --resource-group teamflow-rg \
  --name teamflowregistry \
  --sku Basic
```

### **Step 2: Build and Push Docker Image**

```bash
# Build image
docker build -t teamflow-api:latest .

# Tag image
docker tag teamflow-api:latest teamflowregistry.azurecr.io/teamflow-api:latest

# Login to registry
az acr login --name teamflowregistry

# Push image
docker push teamflowregistry.azurecr.io/teamflow-api:latest
```

### **Step 3: Deploy Container Instance**

```bash
az container create \
  --resource-group teamflow-rg \
  --name teamflow-api \
  --image teamflowregistry.azurecr.io/teamflow-api:latest \
  --environment-variables \
    ConnectionStrings__DefaultConnection="YOUR_CONNECTION_STRING" \
    Jwt__Secret="your-secret" \
  --cpu 1 \
  --memory 1 \
  --ports 5286 \
  --registry-login-server teamflowregistry.azurecr.io \
  --registry-username <username> \
  --registry-password <password>
```

### **Step 4: Get Public IP**

```bash
az container show \
  --resource-group teamflow-rg \
  --name teamflow-api \
  --query ipAddress.fqdn
```

---

## **☸️ OPTION 3: AZURE KUBERNETES SERVICE (AKS)**

### **Step 1: Create AKS Cluster**

```bash
az aks create \
  --resource-group teamflow-rg \
  --name teamflow-aks \
  --node-count 2 \
  --vm-set-type VirtualMachineScaleSets \
  --load-balancer-sku standard \
  --enable-managed-identity \
  --network-plugin azure \
  --network-policy azure
```

### **Step 2: Get Credentials**

```bash
az aks get-credentials \
  --resource-group teamflow-rg \
  --name teamflow-aks
```

### **Step 3: Deploy Using kubectl**

We already have Kubernetes manifests ready. Deploy with:

```bash
kubectl apply -f k8s/production/config.yaml

# Monitor deployment
kubectl get pods
kubectl get svc
kubectl logs -f deployment/mutisaasapp
```

---

## **🔄 OPTION 4: GITHUB ACTIONS → AZURE (AUTOMATED)**

### **Step 1: Create GitHub Secrets**

In your GitHub repo, go to **Settings → Secrets and variables → Actions** and add:

```
AZURE_SUBSCRIPTION_ID=your-subscription-id
AZURE_RESOURCE_GROUP=teamflow-rg
AZURE_WEBAPP_NAME=teamflow-api
AZURE_REGISTRY_URL=teamflowregistry.azurecr.io
AZURE_REGISTRY_USERNAME=your-registry-username
AZURE_REGISTRY_PASSWORD=your-registry-password
```

### **Step 2: Create GitHub Actions Workflow**

Create `.github/workflows/deploy-azure.yml`:

```yaml
name: Deploy to Azure

on:
  push:
    branches: [master]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '10.0'
    
    - name: Build
      run: dotnet build MutiSaaSApp/MutiSaaSApp.csproj -c Release
    
    - name: Test
      run: dotnet test MultiSaasTest/MultiSaasTest.csproj -c Release
    
    - name: Publish
      run: dotnet publish MutiSaaSApp/MutiSaaSApp.csproj -c Release -o ./publish
    
    - name: Build Docker Image
      run: |
        docker build -t ${{ secrets.AZURE_REGISTRY_URL }}/teamflow-api:${{ github.sha }} .
        docker tag ${{ secrets.AZURE_REGISTRY_URL }}/teamflow-api:${{ github.sha }} ${{ secrets.AZURE_REGISTRY_URL }}/teamflow-api:latest
    
    - name: Push to Azure Registry
      run: |
        docker login -u ${{ secrets.AZURE_REGISTRY_USERNAME }} -p ${{ secrets.AZURE_REGISTRY_PASSWORD }} ${{ secrets.AZURE_REGISTRY_URL }}
        docker push ${{ secrets.AZURE_REGISTRY_URL }}/teamflow-api:${{ github.sha }}
        docker push ${{ secrets.AZURE_REGISTRY_URL }}/teamflow-api:latest
    
    - name: Deploy to Azure App Service
      uses: azure/webapps-deploy@v2
      with:
        app-name: ${{ secrets.AZURE_WEBAPP_NAME }}
        publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
        images: ${{ secrets.AZURE_REGISTRY_URL }}/teamflow-api:latest
```

### **Step 3: Get Publish Profile**

```bash
# Download publish profile
az webapp deployment list-publishing-profiles \
  --resource-group teamflow-rg \
  --name teamflow-api \
  --query "[0]" > profile.xml
```

Add the contents as `AZURE_PUBLISH_PROFILE` secret in GitHub.

---

## **🔗 DATABASE MIGRATIONS**

After deployment, run migrations:

```bash
# Local: Create migration
dotnet ef migrations add InitialCreate --project Infastructure --startup-project MutiSaaSApp

# Apply to Azure Database
dotnet ef database update --project Infastructure --startup-project MutiSaaSApp -- "Server=tcp:teamflow-sqlserver.database.windows.net,1433;Initial Catalog=TeamFlowDb;Persist Security Info=False;User ID=adminuser;Password=YourP@ssw0rd123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

---

## **🛡️ PRODUCTION CONFIGURATION**

Update `appsettings.Production.json` with Azure values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-azure-sql-connection-string"
  },
  "Jwt": {
    "Secret": "your-super-secret-key-from-azure-keyvault",
    "ExpiryMinutes": 60
  },
  "Redis": {
    "Enabled": true,
    "ConnectionString": "your-redis-connection-string"
  },
  "Cors": {
    "AllowedOrigins": ["https://teamflow-roan-rho.vercel.app"]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

---

## **📊 RECOMMENDED SETUP**

For your project, I recommend:

```
┌─────────────────────────────────────┐
│   Azure App Service                 │
│   (teamflow-api.azurewebsites.net) │
├─────────────────────────────────────┤
│   ↓                                 │
│   Azure SQL Database                │
│   (teamflow-sqlserver.database...) │
├─────────────────────────────────────┤
│   ↓                                 │
│   Azure Redis Cache                 │
│   (teamflow-redis.redis.cache...)  │
├─────────────────────────────────────┤
│   ↓                                 │
│   GitHub Actions CI/CD              │
│   (Automatic deployment on push)   │
└─────────────────────────────────────┘
```

**Total Cost:** ~$100-150/month

---

## **✅ VERIFICATION AFTER DEPLOYMENT**

```bash
# 1. Check app health
curl https://teamflow-api.azurewebsites.net/api/health

# 2. Check Swagger
https://teamflow-api.azurewebsites.net/swagger

# 3. Test login endpoint
curl -X POST https://teamflow-api.azurewebsites.net/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"password"}'

# 4. Check logs
az webapp log tail --resource-group teamflow-rg --name teamflow-api

# 5. Monitor metrics
az monitor metrics list \
  --resource-group teamflow-rg \
  --resource teamflow-api \
  --resource-type "Microsoft.Web/sites"
```

---

## **🚨 TROUBLESHOOTING**

### **App won't start**
```bash
# Check logs
az webapp log tail --resource-group teamflow-rg --name teamflow-api

# Restart app
az webapp restart --resource-group teamflow-rg --name teamflow-api
```

### **Database connection fails**
```bash
# Test SQL connection
sqlcmd -S teamflow-sqlserver.database.windows.net -U adminuser -P "YourP@ssw0rd123!" -d TeamFlowDb -Q "SELECT 1"

# Check firewall rules
az sql server firewall-rule list --resource-group teamflow-rg --server teamflow-sqlserver
```

### **CORS still not working**
- Verify frontend URL in App Settings
- Check CORS middleware in Program.cs
- Clear browser cache and restart

---

## **💰 COST OPTIMIZATION**

```
App Service B2:      $50/month
SQL Database S1:     $15/month
Redis Cache (Basic): $20/month
Storage:             $5/month
─────────────────────────────
Total:               ~$90/month
```

**Save money:**
- Use `B1` SKU for dev ($10/month)
- Enable auto-shutdown when not in use
- Use shared database for multiple apps

---

## **📞 QUICK REFERENCE URLS**

After deployment:

```
API Base:        https://teamflow-api.azurewebsites.net
Swagger Docs:    https://teamflow-api.azurewebsites.net/swagger
Health Check:    https://teamflow-api.azurewebsites.net/api/health
```

---

## **🎯 NEXT STEPS**

1. Choose deployment option (I recommend **Option 1: App Service**)
2. Run the Azure CLI commands
3. Configure app settings
4. Deploy application
5. Run migrations
6. Test endpoints
7. Monitor logs
8. Set up CI/CD pipeline

---

**Ready to deploy?** Start with:
```bash
az login
az group create --name teamflow-rg --location eastus
```

Let me know which option you choose! 🚀
