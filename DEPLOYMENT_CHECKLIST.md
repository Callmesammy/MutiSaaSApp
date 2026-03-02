# ✅ PRODUCTION DEPLOYMENT — CONFIRMATION CHECKLIST

> Print this page or bookmark for reference during deployment

---

## **PHASE 1: GitHub Secrets Setup** ⏱️ ~10 minutes

### **Gather Information**

```
📋 SONAR_TOKEN
   ☐ Visited: https://sonarcloud.io/account/security/
   ☐ Generated token
   ☐ Format: squ_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
   Status: ⏳ TODO

📋 KUBECONFIG (Base64 Encoded)
   ☐ Got kubeconfig file from cluster admin
   ☐ Encoded to base64:
      Linux/Mac: cat ~/.kube/config | base64 -w 0
      Windows: [Convert]::ToBase64String(...)
   ☐ Copied full encoded string
   Status: ⏳ TODO

📋 GHCR_TOKEN
   ☐ Went to: GitHub → Settings → Developer settings → Tokens
   ☐ Generated new token
   ☐ Selected scopes: write:packages, read:packages
   ☐ Format: ghp_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
   Status: ⏳ TODO

📋 GHCR_USERNAME
   ☐ Your GitHub username: __________________
   Status: ⏳ TODO
```

### **Add Secrets to GitHub**

```
🔗 Navigate to: https://github.com/Callmesammy/MutiSaaSApp/settings/secrets/actions

For each secret:
☐ Click "New repository secret"
☐ Enter Name: SONAR_TOKEN
☐ Enter Value: <paste full token>
☐ Click "Add secret"

☐ Repeat for: KUBECONFIG
☐ Repeat for: GHCR_TOKEN
☐ Repeat for: GHCR_USERNAME

Verification:
☐ SONAR_TOKEN appears in list
☐ KUBECONFIG appears in list
☐ GHCR_TOKEN appears in list
☐ GHCR_USERNAME appears in list
```

**Phase 1 Complete? ✅ YES ☐  NO ☐**

---

## **PHASE 2: Kubernetes Cluster Setup** ⏱️ ~10 minutes

### **Prerequisites**

```
✓ kubectl installed
  ☐ Verify: kubectl version
  ☐ Status: OK ✓ / FAILED ✗

✓ Connected to Kubernetes cluster
  ☐ Verify: kubectl cluster-info
  ☐ Status: OK ✓ / FAILED ✗

✓ Have cluster admin access
  ☐ Can create namespaces
  ☐ Can create secrets
  ☐ Status: OK ✓ / FAILED ✗
```

### **Run Setup Script**

```bash
🔧 COMMAND:
   chmod +x scripts/setup-production.sh
   ./scripts/setup-production.sh

📝 SCRIPT WILL PROMPT FOR:
   ☐ GitHub username
   ☐ GHCR token (paste GHCR_TOKEN value)
   ☐ Email address
   ☐ SQL Server connection string
      Example: Server=my-db.database.windows.net;Database=MutiSaaS_Prod;User Id=sa;Password=...;
   ☐ Redis connection string
      Example: redis.example.com:6379
   ☐ JWT Secret (32+ characters)

✅ WHAT GETS CREATED:
   ☐ Namespace: mutisaas-production
   ☐ Secret: ghcr-secret (registry)
   ☐ Secret: app-secrets (database/cache/jwt)
   ☐ Validates Kubernetes manifests
```

### **Verify Setup**

```bash
✓ Check namespace:
  kubectl get namespace mutisaas-production
  ☐ Status: Active

✓ Check secrets:
  kubectl get secrets -n mutisaas-production
  ☐ Should show: default-token-xxxxx, ghcr-secret, app-secrets

✓ Dry-run deployment:
  kubectl apply --dry-run=client -f k8s/production/config.yaml
  ☐ Status: Should show 'dry-run=client ... success'
```

**Phase 2 Complete? ✅ YES ☐  NO ☐**

---

## **PHASE 3: Deploy to Production** ⏱️ ~5 minutes

### **Push Code to Master**

```bash
🔧 COMMANDS:
   git checkout master
   ☐ Switched to branch 'master'

   git add .
   ☐ Changes staged

   git commit -m "🚀 Deploy to production - V4 complete"
   ☐ Commit created

   git push origin master
   ☐ Code pushed to GitHub
```

### **GitHub Actions Workflow Starts**

```
🔄 WORKFLOW PROGRESS:

Job 1: build-and-test
   ☐ Build started
   ☐ Dependencies restored
   ☐ Application built
   ☐ 41 tests executed
   ☐ All tests passed ✅
   Estimated: 3-5 minutes

Job 2: build-docker
   ☐ Docker image built
   ☐ Image pushed to ghcr.io
   Estimated: 2-3 minutes

Job 3: code-quality
   ☐ SonarCloud analysis
   ☐ Quality gates checked
   Estimated: 2 minutes

Job 4: security-scan
   ☐ Trivy scan completed
   ☐ Vulnerabilities assessed
   Estimated: 1 minute

Job 5: deploy-staging (optional)
   ☐ Staging deployment
   Estimated: 1-2 minutes

Job 6: deploy-production
   ☐ Waiting for approval ⏳
   ☐ DO NOT PROCEED until workflow reaches this job
```

### **Approve Production Deployment**

```
✅ STEP 1: Go to Actions Tab
   🔗 https://github.com/Callmesammy/MutiSaaSApp/actions
   ☐ Visited Actions page
   ☐ Found latest workflow run

✅ STEP 2: Click on Workflow Run
   ☐ Clicked on the run
   ☐ Waiting for "Review deployments" button to appear
   ⏱️  Note: Only appears after previous jobs complete

✅ STEP 3: Review & Approve
   ☐ Found "Review deployments" button
   ☐ Clicked it
   ☐ Selected environment: "mutisaas-production"
   ☐ Clicked "Approve and deploy"
   ☐ Deployment started

Estimated wait: 5-10 minutes from push to approval button
```

**Phase 3 Complete? ✅ YES ☐  NO ☐**

---

## **PHASE 4: Verify Deployment** ⏱️ ~5 minutes

### **Monitor Pod Deployment**

```bash
🔧 COMMAND:
   kubectl get pods -n mutisaas-production -w
   
✅ EXPECTED OUTPUT:
   mutisaas-api-xxxxx    1/1     Running     0        2m
   mutisaas-api-yyyyy    1/1     Running     0        1m
   mutisaas-api-zzzzz    1/1     Running     0        30s

⏱️  WAIT UNTIL:
   ☐ At least 3 pods showing "Running"
   ☐ All have "1/1" ready
   ☐ No restarts (RESTARTS = 0)
   ☐ Press Ctrl+C to exit
```

### **Check Application Logs**

```bash
🔧 COMMAND:
   kubectl logs -f deployment/mutisaas-api -n mutisaas-production

✅ EXPECTED OUTPUT (first few lines):
   info: MutiSaaSApp.Program[0] Application started
   info: Microsoft.Hosting.Lifetime[0] Now listening on: https://+:443
   info: LogContextMiddleware enabled

⚠️  WATCH FOR:
   ☐ No ERROR level messages in first 30 seconds
   ☐ Health checks passing
   ☐ Database connection successful
   ☐ Press Ctrl+C to exit
```

### **Test Health Endpoint**

```bash
🔧 COMMAND (in new terminal):
   kubectl port-forward svc/mutisaas-api 8080:80 -n mutisaas-production
   
   (In another terminal)
   curl http://localhost:8080/api/health

✅ EXPECTED RESPONSE:
   {
     "success": true,
     "data": {
       "status": "Healthy",
       "timestamp": "2024-01-XX...",
       "database": { "status": "Healthy", "responseTimeMs": 45 },
       "cache": { "status": "Healthy", "responseTimeMs": 12 }
     }
   }

✅ VERIFY:
   ☐ HTTP Status: 200 OK
   ☐ status: "Healthy"
   ☐ database health: "Healthy"
   ☐ cache health: "Healthy"
```

### **Check Service Endpoint**

```bash
🔧 COMMAND:
   kubectl get svc -n mutisaas-production

✅ EXPECTED OUTPUT:
   NAME           TYPE           CLUSTER-IP    EXTERNAL-IP     PORT(S)
   mutisaas-api   LoadBalancer   10.0.1.2      XX.XX.XX.XX     80:32123/TCP

✅ NOTE:
   ☐ If EXTERNAL-IP shows "pending", wait 1-2 minutes
   ☐ Once assigned, you can access externally
   ☐ Use: curl http://XX.XX.XX.XX/api/health
```

### **Monitor Auto-Scaling**

```bash
🔧 COMMAND:
   kubectl get hpa -n mutisaas-production

✅ EXPECTED OUTPUT:
   NAME           REFERENCE              TARGETS     MINPODS   MAXPODS
   mutisaas-api   Deployment/mutisaas    45%/70%     3         10

✅ VERIFY:
   ☐ HPA is active
   ☐ Current replicas: 3 (minimum)
   ☐ Will scale to 10 if CPU > 70% or Memory > 80%
```

**Phase 4 Complete? ✅ YES ☐  NO ☐**

---

## **PHASE 5: Post-Deployment Validation** ⏱️ ~10 minutes

### **Success Criteria**

```
✅ MUST HAVE:
   ☐ 3+ pods in Running state
   ☐ Health endpoint returns 200 OK
   ☐ Database health: Passing
   ☐ Cache health: Passing
   ☐ No pod restarts
   ☐ No ERROR logs in first 5 minutes
   ☐ GitHub Actions workflow shows ✅ on all jobs

✅ SHOULD HAVE:
   ☐ Service has external IP assigned
   ☐ HPA is monitoring and ready
   ☐ Logs are flowing smoothly
   ☐ Response times < 500ms

⚠️  RED FLAGS (if any occur):
   ☐ Pod stuck in CrashLoopBackOff
   ☐ ImagePullBackOff error
   ☐ Connection refused to database/cache
   ☐ Health endpoint returns 503
   ☐ ERROR level logs appearing
   → If any red flags: Stop and troubleshoot (see guide)
```

### **Collect Baseline Metrics**

```bash
🔧 COMMAND:
   kubectl top pod -n mutisaas-production
   kubectl top pod -n mutisaas-production --containers

📊 RECORD THESE VALUES:
   CPU Usage (avg): __________ m
   Memory Usage (avg): __________ Mi
   Highest CPU pod: __________
   Highest Memory pod: __________

💾 SAVE FOR COMPARISON:
   kubectl top pod -n mutisaas-production > deployment-baseline.txt
```

### **Notification**

```
📢 INFORM STAKEHOLDERS:
   ☐ Team lead
   ☐ Product manager
   ☐ Customer success
   ☐ Support team

📝 MESSAGE TEMPLATE:
   "MutiSaas API successfully deployed to production!
    - V4 features: 5/5 complete
    - Tests: 41/41 passing
    - Replicas: 3 (auto-scales to 10)
    - Health: All systems operational
    - Monitoring: Enabled and active"
```

**Phase 5 Complete? ✅ YES ☐  NO ☐**

---

## **🎉 DEPLOYMENT COMPLETE!**

```
OVERALL STATUS: ✅ PRODUCTION DEPLOYMENT SUCCESSFUL

Timeline:
- Phase 1 (Secrets): _____ min
- Phase 2 (Kubernetes): _____ min
- Phase 3 (Deploy): _____ min
- Phase 4 (Verify): _____ min
- Phase 5 (Validate): _____ min
TOTAL TIME: _____ min

Date/Time Started: _______________________
Date/Time Completed: _______________________
Deployed By: _______________________
Approved By: _______________________
```

---

## **📋 ROLLBACK PROCEDURE** (if needed)

```bash
🚨 ONLY IF CRITICAL ISSUE OCCURS:

   kubectl rollout undo deployment/mutisaas-api -n mutisaas-production
   
   ☐ Rollback started
   ☐ Previous version restoring
   
   Verify:
   kubectl get pods -n mutisaas-production
   ☐ New pods appearing
   ☐ Old pods terminating
   
   Estimated time: 2-3 minutes

   After rollback, STOP and investigate issues in application logs
```

---

## **📞 TROUBLESHOOTING QUICK REFERENCE**

| Issue | Command | Solution |
|-------|---------|----------|
| Pod won't start | `kubectl describe pod <pod-name> -n mutisaas-production` | Check image, resources, secrets |
| Image pull failed | `kubectl get secrets ghcr-secret -n mutisaas-production` | Verify registry credentials |
| Connection refused | `kubectl exec -it <pod-name> -n mutisaas-production -- /bin/sh` | Test DB/Redis connectivity |
| High memory | `kubectl top pod -n mutisaas-production --containers` | Check for memory leaks, increase limits |
| Deployment slow | Check GitHub Actions logs | May need faster runner or more resources |

---

## **✅ SIGN-OFF**

```
I confirm that:

☐ All pre-deployment checks completed
☐ GitHub Secrets configured correctly
☐ Kubernetes cluster is ready
☐ Database/Cache connections working
☐ All 4 phases completed successfully
☐ Health checks passing
☐ Rollback procedure understood

Deployment Date: _______________________
Deployment Owner: _______________________
Reviewed By: _______________________

APPROVED FOR PRODUCTION: ✅ YES ☐  / ❌ NO ☐
```

---

**🚀 Ready? Start Phase 1 now!**

For detailed information, see:
- `PRODUCTION_QUICK_START.md` - Step-by-step guide
- `DEPLOYMENT_GUIDE.md` - Comprehensive reference
- `GITHUB_SECRETS_SETUP.md` - Secrets configuration

Good luck! 🎉
