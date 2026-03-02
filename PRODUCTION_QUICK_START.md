# ✅ **PRODUCTION DEPLOYMENT — QUICK START**

> Complete checklist to deploy to production in ~30 minutes

---

## **📋 PRE-FLIGHT CHECKLIST** (5 min)

- [ ] **Application Status:**
  - Build: ✅ SUCCESS
  - Tests: ✅ 41/41 PASSING
  - Docker: ✅ Multi-stage configured
  - Kubernetes: ✅ Manifests ready

- [ ] **Team Coordination:**
  - [ ] Notified team of deployment
  - [ ] Backup plan reviewed
  - [ ] Rollback procedure understood

- [ ] **Documentation:**
  - [ ] Read `DEPLOYMENT_GUIDE.md`
  - [ ] Read `GITHUB_SECRETS_SETUP.md`
  - [ ] Bookmarked support links

---

## **🔐 PHASE 1: GitHub Secrets Setup** (10 min)

### **Step 1.1: Prepare Secrets**

Gather these values:

```
✓ SONAR_TOKEN
  From: https://sonarcloud.io/account/security/
  Format: squ_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
  Status: ⏳ TODO
  
✓ KUBECONFIG
  From: Kubernetes cluster admin or ~/.kube/config
  Format: base64 encoded YAML
  Status: ⏳ TODO
  
✓ GHCR_TOKEN
  From: GitHub → Settings → Developer settings → Personal tokens
  Format: ghp_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
  Status: ⏳ TODO
  
✓ GHCR_USERNAME
  From: Your GitHub username
  Format: callmesammy
  Status: ⏳ TODO
```

### **Step 1.2: Add to GitHub**

1. Go to: https://github.com/Callmesammy/MutiSaaSApp/settings/secrets/actions
2. Click **"New repository secret"**
3. Add each secret:
   - Name: `SONAR_TOKEN` → Value: `squ_...`
   - Name: `KUBECONFIG` → Value: `base64_encoded...`
   - Name: `GHCR_TOKEN` → Value: `ghp_...`
   - Name: `GHCR_USERNAME` → Value: `callmesammy`

**Verification:** All 4 secrets appear in the list

```
✓ SONAR_TOKEN
✓ KUBECONFIG
✓ GHCR_TOKEN
✓ GHCR_USERNAME
```

---

## **☸️ PHASE 2: Kubernetes Cluster Preparation** (10 min)

### **Step 2.1: Run Setup Script**

Make script executable and run it:

```bash
chmod +x scripts/setup-production.sh
./scripts/setup-production.sh
```

**This will prompt for:**
- GitHub username
- GHCR token
- Email address
- SQL Server connection string
- Redis connection string
- JWT Secret

**What it creates:**
- [ ] Namespace: `mutisaas-production`
- [ ] Docker registry secret: `ghcr-secret`
- [ ] Application secrets: `app-secrets`
- [ ] Validates manifests

### **Step 2.2: Verify Setup**

```bash
# Check namespace
kubectl get namespace mutisaas-production

# Check secrets
kubectl get secrets -n mutisaas-production
# Should show:
# - default-token-xxxxx
# - ghcr-secret
# - app-secrets
```

---

## **🚀 PHASE 3: Deploy to Production** (5 min)

### **Step 3.1: Push Code to Master**

```bash
# Ensure you're on master branch
git checkout master

# Add all changes
git add .

# Commit
git commit -m "🚀 Deploy to production - V4 complete"

# Push to master (triggers GitHub Actions)
git push origin master
```

### **Step 3.2: Watch GitHub Actions**

1. Go to: https://github.com/Callmesammy/MutiSaaSApp/actions
2. Click the latest workflow run
3. Watch progress:
   - ✅ **build-and-test** → Runs 41 tests
   - ✅ **build-docker** → Creates Docker image
   - ✅ **code-quality** → SonarCloud analysis
   - ✅ **security-scan** → Trivy vulnerability scan
   - ⏳ **deploy-staging** → Optional staging deployment
   - ⏳ **deploy-production** → **Requires approval**

### **Step 3.3: Approve Production Deployment**

When workflow reaches `deploy-production` job:

1. Click **Review deployments**
2. Select `mutisaas-production` environment
3. Click **Approve and deploy**
4. Workflow continues → Deploys to Kubernetes

**Estimated time:** 5-10 minutes total

---

## **✅ PHASE 4: Verify Deployment** (5 min)

### **Step 4.1: Check Pod Status**

```bash
# Watch pods come up
kubectl get pods -n mutisaas-production -w
# Press Ctrl+C when 3+ pods are RUNNING

# Expected output:
# mutisaas-api-xxxxx        1/1     Running     0        2m
# mutisaas-api-yyyyy        1/1     Running     0        1m
# mutisaas-api-zzzzz        1/1     Running     0        30s
```

### **Step 4.2: Check Logs**

```bash
# Follow deployment logs
kubectl logs -f deployment/mutisaas-api -n mutisaas-production

# Should see:
# info: MutiSaaSApp.Program[0] Application started
# info: Microsoft.Hosting.Lifetime[0] Now listening on: https://+:443
```

### **Step 4.3: Test Health Endpoint**

```bash
# Port-forward the service
kubectl port-forward svc/mutisaas-api 8080:80 -n mutisaas-production

# In another terminal, test health
curl http://localhost:8080/api/health

# Expected response:
# {
#   "success": true,
#   "data": {
#     "status": "Healthy",
#     "timestamp": "2024-01-XX...",
#     "database": { "status": "Healthy", "responseTimeMs": 45 },
#     "cache": { "status": "Healthy", "responseTimeMs": 12 }
#   }
# }
```

### **Step 4.4: Check Service Endpoint**

```bash
# Get service details
kubectl get svc -n mutisaas-production

# Expected:
# NAME           TYPE           CLUSTER-IP      EXTERNAL-IP     PORT(S)
# mutisaas-api   LoadBalancer   10.0.XX.XX      XX.XX.XX.XX     80:xxxxx/TCP

# Test external endpoint (if EXTERNAL-IP is assigned)
curl http://XX.XX.XX.XX/api/health
```

---

## **📊 PHASE 5: Monitoring Setup** (Optional, 5-10 min)

### **Step 5.1: Enable Pod Auto-Scaling**

```bash
# Verify HPA is running
kubectl get hpa -n mutisaas-production
# Should show: mutisaas-api with MIN: 3, MAX: 10

# Watch HPA metrics
kubectl get hpa mutisaas-api -n mutisaas-production -w
```

### **Step 5.2: Setup Alerts** (If monitoring system available)

Configure alerts for:
- Pod restart count > 0
- CPU usage > 80%
- Memory usage > 85%
- Error rate > 1%
- Response time > 2s

### **Step 5.3: Collect Baseline Metrics**

```bash
# CPU usage
kubectl top pod -n mutisaas-production

# Memory usage
kubectl top pod -n mutisaas-production --containers

# Save baseline for comparison
kubectl top pod -n mutisaas-production > baseline-metrics.txt
```

---

## **🎯 SUCCESS CRITERIA**

✅ **Deployment is successful when:**

- [ ] All pods are `RUNNING` (at least 3)
- [ ] No pod restarts in last 5 minutes
- [ ] Health endpoint returns 200 OK
- [ ] Database health check: PASSING
- [ ] Cache health check: PASSING
- [ ] Logs show no ERROR level messages
- [ ] GitHub Actions workflow shows ✅ on all jobs
- [ ] Load balancer has assigned external IP (if applicable)

---

## **🆘 TROUBLESHOOTING**

### **Pod won't start?**
```bash
kubectl describe pod <pod-name> -n mutisaas-production
kubectl logs <pod-name> -n mutisaas-production
# Check for: ImagePullBackOff, CrashLoopBackOff
```

### **ImagePullBackOff error?**
```bash
# Verify registry secret
kubectl get secrets ghcr-secret -n mutisaas-production

# Check credentials
kubectl describe secret ghcr-secret -n mutisaas-production

# Solution: Recreate secret with correct credentials
./scripts/setup-production.sh
```

### **Connection refused to database/cache?**
```bash
# Verify connection strings in secrets
kubectl get secret app-secrets -n mutisaas-production -o yaml

# Check database/cache are accessible from pod
kubectl exec -it <pod-name> -n mutisaas-production -- /bin/sh
# Inside pod: curl http://your-db-host
```

### **High memory usage?**
```bash
# Check current limits
kubectl get deployment mutisaas-api -n mutisaas-production -o yaml | grep -A 2 "resources:"

# Update limits in k8s/production/config.yaml
# Redeploy with: kubectl apply -f k8s/production/config.yaml
```

---

## **⏮️ ROLLBACK (If needed)**

```bash
# Quick rollback to previous version
kubectl rollout undo deployment/mutisaas-api -n mutisaas-production

# Verify rollback
kubectl get deployment mutisaas-api -n mutisaas-production -o jsonpath='{.spec.template.spec.containers[0].image}'

# Expected: Previous image tag shows
```

---

## **📋 FINAL CHECKLIST**

After deployment is verified:

- [ ] Team notified of successful deployment
- [ ] Monitoring alerts are working
- [ ] Logs are being collected
- [ ] Health checks passing consistently
- [ ] Database backups verified
- [ ] Rollback procedure tested
- [ ] Post-deployment metrics collected
- [ ] Customer-facing endpoints tested
- [ ] Performance baselines recorded
- [ ] Documentation updated

---

## **🎉 DEPLOYMENT COMPLETE**

**Timeline Summary:**
- Phase 1 (Secrets): 10 min
- Phase 2 (Kubernetes): 10 min
- Phase 3 (Deploy): 5 min
- Phase 4 (Verify): 5 min
- Phase 5 (Monitoring): 5-10 min

**Total:** ~35-40 minutes ⏱️

**Next Steps:**
1. Monitor for 24 hours
2. Gather performance metrics
3. Plan V3 feature implementation

---

**🚀 Ready to deploy? Start with Phase 1!**

Questions? See:
- `DEPLOYMENT_GUIDE.md` - Comprehensive guide
- `GITHUB_SECRETS_SETUP.md` - Secrets configuration
- `.github/workflows/ci-cd.yml` - Workflow reference
- `k8s/production/config.yaml` - Kubernetes config
