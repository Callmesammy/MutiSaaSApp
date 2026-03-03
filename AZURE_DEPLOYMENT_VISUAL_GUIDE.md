# 🎯 DEPLOYMENT DECISION TREE

## **Which Azure Option Should I Choose?**

```
START HERE
    ↓
Do you have complex microservices
or need advanced container orchestration?
    ├─ YES → Use Azure Kubernetes Service (AKS)
    │        (Option 3: Most powerful, ~$100+/month, 30 min setup)
    │
    └─ NO
        ↓
    Do you want automatic CI/CD on every git push?
        ├─ YES → Use GitHub Actions + App Service
        │        (Option 4: Most automated, variable cost, 20 min setup)
        │
        └─ NO
            ↓
        Do you want the simplest setup?
            ├─ YES → Use Azure App Service ⭐ RECOMMENDED
            │        (Option 1: Easiest, ~$90/month, 15 min setup)
            │
            └─ NO → Use Container Instances
                   (Option 2: Middle ground, ~$20/month, 10 min setup)
```

---

## **🚀 STEP-BY-STEP: APP SERVICE DEPLOYMENT**

### **PHASE 1: SETUP (5 minutes)**

```bash
# Login to Azure
$ az login
# Browser opens, authenticate with your Azure account

# Set default subscription
$ az account set --subscription "YOUR_SUBSCRIPTION_ID"

# Create resource group (container for all resources)
$ az group create --name teamflow-rg --location eastus
# Created: Resource group 'teamflow-rg' in location 'eastus'

# Create App Service Plan (defines compute capacity)
$ az appservice plan create \
    --name teamflow-plan \
    --resource-group teamflow-rg \
    --sku B2 \
    --is-linux
# Created: App Service plan 'teamflow-plan'
```

**✅ After Phase 1:** You have infrastructure ready

---

### **PHASE 2: CONFIGURE DATABASE (3 minutes)**

```bash
# Create SQL Server
$ az sql server create \
    --name teamflow-sqlserver \
    --resource-group teamflow-rg \
    --location eastus \
    --admin-user adminuser \
    --admin-password "SecureP@ss123!"
# Created: SQL Server 'teamflow-sqlserver'

# Create Database
$ az sql db create \
    --resource-group teamflow-rg \
    --server teamflow-sqlserver \
    --name TeamFlowDb \
    --service-objective S1
# Created: Database 'TeamFlowDb'

# Get connection string
$ az sql db show-connection-string \
    --client ado.net \
    --server teamflow-sqlserver \
    --name TeamFlowDb
# Connection string: Server=tcp:teamflow-sqlserver.database.windows.net,1433;...
```

**✅ After Phase 2:** Database is ready

---

### **PHASE 3: CREATE WEB APP (2 minutes)**

```bash
# Create Web App
$ az webapp create \
    --resource-group teamflow-rg \
    --plan teamflow-plan \
    --name teamflow-api-prod
# Created: Web app 'teamflow-api-prod'
# URL: https://teamflow-api-prod.azurewebsites.net

# Configure connection string
$ az webapp config appsettings set \
    --resource-group teamflow-rg \
    --name teamflow-api-prod \
    --settings \
      "ConnectionStrings__DefaultConnection=YOUR_CONNECTION_STRING" \
      "Jwt__Secret=your-super-secret-key" \
      "ASPNETCORE_ENVIRONMENT=Production"
# Settings updated
```

**✅ After Phase 3:** App Service configured

---

### **PHASE 4: DEPLOY APPLICATION (3 minutes)**

```bash
# Publish application
$ dotnet publish -c Release -o ./publish
# Published to: ./publish

# Create deployment package
$ cd publish
$ zip -r ../app.zip .
$ cd ..
# Created: app.zip (50 MB)

# Deploy to Azure
$ az webapp up \
    --resource-group teamflow-rg \
    --name teamflow-api-prod \
    --package ./app.zip
# Deployment succeeded!
# URL: https://teamflow-api-prod.azurewebsites.net
```

**✅ After Phase 4:** Your app is live!

---

### **PHASE 5: VERIFY DEPLOYMENT (2 minutes)**

```bash
# Test health endpoint
$ curl https://teamflow-api-prod.azurewebsites.net/api/health
# Response: { "status": "Healthy" }

# View logs
$ az webapp log tail --resource-group teamflow-rg --name teamflow-api-prod
# [timestamp] Application starting...
# [timestamp] Application started successfully

# Open Swagger
$ open https://teamflow-api-prod.azurewebsites.net/swagger
# Swagger UI loads in browser
```

**✅ After Phase 5:** Everything working!

---

## **📊 ARCHITECTURE AFTER DEPLOYMENT**

```
┌──────────────────────────────────────────────────────────┐
│                   PRODUCTION SETUP                       │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  Frontend (Next.js)                 Backend (ASP.NET)   │
│  teamflow-roan-rho.vercel.app ─────→ teamflow-api-prod │
│  (Deployed to Vercel)       (CORS)   (Azure App Serv)  │
│                                                          │
│                           ↓                              │
│                      Database (SQL)                      │
│                teamflow-sqlserver (Azure)               │
│                                                          │
│  GitHub Repo                         CI/CD Pipeline     │
│  (master branch) ─────────────────→  GitHub Actions    │
│                  (auto-deploy)        ↓                │
│                                 Azure App Service      │
│                                 (auto-update)          │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

---

## **🔍 MONITORING AFTER DEPLOYMENT**

### **Check Application Health**

```bash
# View real-time logs
az webapp log tail --resource-group teamflow-rg --name teamflow-api-prod

# Monitor metrics
az monitor metrics list \
  --resource-group teamflow-rg \
  --resource teamflow-api-prod \
  --resource-type "Microsoft.Web/sites" \
  --metric "Http2xx" "Http5xx"

# Restart if needed
az webapp restart --resource-group teamflow-rg --name teamflow-api-prod
```

### **Database Maintenance**

```bash
# Backup database
az sql db copy \
  --resource-group teamflow-rg \
  --server teamflow-sqlserver \
  --name TeamFlowDb \
  --dest-server teamflow-sqlserver \
  --dest-name TeamFlowDb-backup

# Update firewall (allow app to access DB)
az sql server firewall-rule create \
  --resource-group teamflow-rg \
  --server teamflow-sqlserver \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

---

## **💰 COST BREAKDOWN (Monthly)**

```
Azure App Service (B2):     $50.00
SQL Database (S1):          $15.00
Data Storage:               $5.00
Data Transfer:              $5.00
───────────────────────────────────
Monthly Estimate:          ~$75.00
Annual Cost:               ~$900
```

**Cost Optimization Tips:**
- Use B1 ($10/month) for dev/test
- Enable auto-shutdown after hours
- Use shared SQL resources
- Monitor usage and scale down if needed

---

## **🚀 AUTOMATED DEPLOYMENT (CI/CD)**

### **Enable GitHub Actions for Auto-Deploy**

1. **In GitHub, go to Settings → Secrets** and add:
   ```
   AZURE_SUBSCRIPTION_ID=your-sub-id
   AZURE_RESOURCE_GROUP=teamflow-rg
   AZURE_WEBAPP_NAME=teamflow-api-prod
   AZURE_PUBLISH_PROFILE=(download from Azure Portal)
   ```

2. **Every push to `master` will:**
   - Build the code
   - Run tests (41/41)
   - Build Docker image
   - Push to registry
   - Deploy to Azure App Service
   - Verify deployment

3. **Monitor in GitHub:**
   - Go to **Actions** tab
   - Watch real-time deployment
   - See logs if anything fails

---

## **✅ VERIFICATION CHECKLIST**

After deployment, verify:

```
Deployment Verification
├─ [ ] API responds at production URL
├─ [ ] Swagger documentation loads
├─ [ ] Health check endpoint works
├─ [ ] Database connection works
├─ [ ] Login endpoint responds
├─ [ ] CORS allows frontend requests
├─ [ ] JWT tokens validate correctly
├─ [ ] Tasks endpoints work (GET, POST, PUT, DELETE)
├─ [ ] Error handling returns proper responses
└─ [ ] Logs show no errors

Frontend Verification
├─ [ ] Frontend connects to production API
├─ [ ] Registration flow works
├─ [ ] Login flow works
├─ [ ] Tasks can be created/edited/deleted
├─ [ ] Members can be invited
├─ [ ] Multi-user collaboration works
└─ [ ] No console errors in browser

Production Quality
├─ [ ] HTTPS enforced
├─ [ ] CORS restricted to frontend domain
├─ [ ] Secrets not in logs
├─ [ ] Structured logging enabled
├─ [ ] Monitoring alerts configured
├─ [ ] Backup schedule set
└─ [ ] Disaster recovery plan ready
```

---

## **⏱️ TIMELINE**

```
Setup Azure CLI           5 min
Create resources          5 min
Configure database        3 min
Create web app           2 min
Deploy code              3 min
Verify deployment        2 min
───────────────────────────────
Total:                  20 min 🎉
```

---

## **🆘 TROUBLESHOOTING**

| Problem | Solution |
|---------|----------|
| **App won't start** | Check logs: `az webapp log tail` |
| **DB connection fails** | Verify connection string in app settings |
| **CORS error in browser** | Clear cache, restart app |
| **Tests fail in CI/CD** | Check environment secrets are set |
| **500 error on API call** | Check application logs for exception |
| **Slow response time** | Scale up App Service plan (B2 → S1) |

---

## **🎓 NEXT STEPS AFTER DEPLOYMENT**

1. **Set up monitoring**
   - Configure Application Insights
   - Create alerts for errors
   - Set up dashboard

2. **Automate deployments**
   - Enable GitHub Actions
   - Test CI/CD pipeline
   - Verify auto-deployment works

3. **Setup backups**
   - Schedule daily backups
   - Test restore procedures
   - Document backup location

4. **Team training**
   - Document deployment process
   - Train team on monitoring
   - Create runbook for troubleshooting

---

## **🎉 YOU'RE DONE!**

Your TeamFlow API is now:
- ✅ Running in production
- ✅ Connected to production database
- ✅ Accessible at `https://teamflow-api-prod.azurewebsites.net`
- ✅ Automatically deploying on git push
- ✅ Monitored and logged
- ✅ Scaled for production load

**Congratulations! 🚀**

