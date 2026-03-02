# 🚀 Production Deployment Guide

> **Status:** Application is PRODUCTION-READY ✅
> **Last Updated:** $(date)

---

## **Pre-Deployment Checklist**

### ✅ Application State
- [x] Build: SUCCESS (0 errors, 0 warnings)
- [x] Tests: 41/41 PASSING (100%)
- [x] Docker: Multi-stage Dockerfile configured
- [x] Health Checks: Enabled (/api/health)
- [x] Logging: Structured (Serilog with context enrichment)
- [x] CI/CD: GitHub Actions workflow ready
- [x] Kubernetes: Manifests prepared (staging & production)

### 🔐 Requirements
- [ ] GitHub Secrets configured
- [ ] Kubernetes cluster access
- [ ] Domain/SSL certificates (optional but recommended)
- [ ] Container registry access (GitHub Container Registry)

---

## **Phase 1: GitHub Secrets Setup** (5 minutes)

### **What You Need**

1. **SONAR_TOKEN** (for code quality analysis)
   - Get it from: https://sonarcloud.io/account/security/
   - Steps:
     1. Sign up/login to SonarCloud
     2. Go to Account → Security → Generate token
     3. Copy token

2. **KUBECONFIG** (for Kubernetes deployment)
   - Get it from your Kubernetes cluster admin
   - Location: typically `~/.kube/config`
   - **IMPORTANT:** Must be base64-encoded before adding to GitHub
   - Steps:
     1. Get config file: `cat ~/.kube/config | base64 -w 0`
     2. Copy entire output

3. **GHCR_TOKEN** (for container registry)
   - Generate Personal Access Token on GitHub
   - Steps:
     1. GitHub → Settings → Developer settings → Personal access tokens
     2. Click "Tokens (classic)" → "Generate new token (classic)"
     3. Select scopes: `write:packages`, `read:packages`
     4. Copy token

4. **GHCR_USERNAME** (your GitHub username)
   - Usually: your GitHub login

---

### **Adding Secrets to GitHub**

```bash
# Navigate to your repository
# Go to: Settings → Secrets and variables → Actions

# Add these secrets:
✓ SONAR_TOKEN=<your_token>
✓ KUBECONFIG=<base64_encoded_config>
✓ GHCR_TOKEN=<your_token>
✓ GHCR_USERNAME=<your_username>
```

**Quick Link:** `https://github.com/Callmesammy/MutiSaaSApp/settings/secrets/actions`

---

## **Phase 2: Environment Configuration** (5 minutes)

### **Production Secrets Template**

Create/Update your `.env.production` file (NOT in GitHub):

```bash
# SQL Server
DatabaseConnection=Server=<your-sql-server>;Database=MutiSaaSApp_Prod;User Id=sa;Password=<strong-password>;Encrypt=true;

# Redis
CacheConnection=<your-redis-host>:6379

# JWT
JwtSecret=<generate-32-char-random-secret>
JwtIssuer=https://yourdomain.com
JwtAudience=yourdomain-users

# Application
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443
Serilog:MinimumLevel:Default=Information

# Docker Registry
REGISTRY=ghcr.io
IMAGE_NAME=<your-github-username>/mutisaasapp
IMAGE_TAG=latest
```

---

## **Phase 3: Kubernetes Cluster Setup** (10 minutes)

### **Prerequisites**
- Kubernetes cluster running (AKS, EKS, GKE, or on-prem)
- `kubectl` installed and configured
- Secrets stored in cluster

### **Create Production Namespace & Secrets**

```bash
# 1. Create namespace
kubectl create namespace mutisaas-production

# 2. Create Docker registry secret (for pulling images from GHCR)
kubectl create secret docker-registry ghcr-secret \
  --docker-server=ghcr.io \
  --docker-username=<github-username> \
  --docker-password=<ghcr-token> \
  --docker-email=<your-email> \
  -n mutisaas-production

# 3. Create application secrets (connection strings, JWT, etc.)
kubectl create secret generic app-secrets \
  --from-literal=DatabaseConnection='<your-connection-string>' \
  --from-literal=CacheConnection='<your-redis-connection>' \
  --from-literal=JwtSecret='<your-jwt-secret>' \
  -n mutisaas-production

# 4. Verify
kubectl get secrets -n mutisaas-production
kubectl get namespace mutisaas-production
```

---

## **Phase 4: Deploy to Production** (2 minutes)

### **Option A: Manual Deployment via GitHub Actions**

1. **Push to master branch:**
   ```bash
   git add .
   git commit -m "Deploy to production"
   git push origin master
   ```

2. **GitHub Actions workflow starts automatically:**
   - Build & test (41 tests run)
   - SonarCloud analysis
   - Docker image build & push to GHCR
   - Security scanning (Trivy)
   - **Deployment requires manual approval** → Check Actions tab

3. **Approve deployment:**
   - Go to: `https://github.com/Callmesammy/MutiSaaSApp/actions`
   - Find latest workflow run
   - Click "Review deployments"
   - Select "mutisaas-production"
   - Click "Approve and deploy"

### **Option B: Manual Deployment via Script**

```bash
# Direct Kubernetes deployment
cd scripts
./deploy-production.sh <docker-image-url>

# Example:
./deploy-production.sh ghcr.io/callmesammy/mutisaasapp:latest
```

---

## **Phase 5: Verify Production Deployment** (5 minutes)

### **Health Checks**

```bash
# 1. Check pods are running
kubectl get pods -n mutisaas-production
# Expected: 3+ pods with RUNNING status

# 2. Check deployment status
kubectl describe deployment mutisaas-api -n mutisaas-production

# 3. Check service endpoint
kubectl get svc -n mutisaas-production

# 4. Test health endpoint (from pod)
kubectl port-forward svc/mutisaas-api 8080:80 -n mutisaas-production
# Then: curl http://localhost:8080/api/health

# 5. Watch logs
kubectl logs -f deployment/mutisaas-api -n mutisaas-production
```

### **Endpoint Health Check**

```bash
# From production environment
curl -X GET https://your-production-domain/api/health

# Expected response:
{
  "success": true,
  "data": {
    "status": "Healthy",
    "timestamp": "2024-01-XX...",
    "database": { "status": "Healthy", "responseTimeMs": XX },
    "cache": { "status": "Healthy", "responseTimeMs": XX }
  }
}
```

---

## **Phase 6: Post-Deployment Monitoring**

### **Logs & Observability**

```bash
# Real-time logs
kubectl logs -f deployment/mutisaas-api -n mutisaas-production --tail=100

# Search logs by context
# (Serilog JSON logs enable structured searching)
kubectl logs deployment/mutisaas-api -n mutisaas-production | grep "UserId"

# Metrics (if Prometheus installed)
# Access dashboard: https://your-prometheus-host
```

### **Alerts to Monitor**

- Pod restarts (liveliness probe failures)
- High CPU/Memory usage
- Failed requests (5xx errors)
- Database connection pool exhaustion
- Cache hit ratio declining

---

## **Phase 7: Rollback Plan** (if needed)

### **Quick Rollback**

```bash
# Rollback to previous deployment
kubectl rollout undo deployment/mutisaas-api -n mutisaas-production

# Verify rollback
kubectl get pods -n mutisaas-production
kubectl get deployment mutisaas-api -n mutisaas-production -o jsonpath='{.spec.template.spec.containers[0].image}'
```

---

## **Deployment Timeline**

| Phase | Task | Time | Status |
|-------|------|------|--------|
| 1 | Add GitHub Secrets | 5 min | ⏳ TODO |
| 2 | Environment Setup | 5 min | ⏳ TODO |
| 3 | Kubernetes Cluster | 10 min | ⏳ TODO |
| 4 | Deploy (GitHub Actions) | 2 min | ⏳ TODO |
| 5 | Verify Health Checks | 5 min | ⏳ TODO |
| 6 | Monitor & Alert Setup | 10 min | ⏳ TODO |
| **TOTAL** | | **37 minutes** | |

---

## **Troubleshooting**

### **Pod won't start?**
```bash
# Check pod status and events
kubectl describe pod <pod-name> -n mutisaas-production
kubectl logs <pod-name> -n mutisaas-production
```

### **Image pull failed?**
```bash
# Verify registry secret exists
kubectl get secrets -n mutisaas-production
# If missing, recreate with correct credentials
```

### **Health check failing?**
```bash
# Check if database/redis are accessible from pod
kubectl exec -it <pod-name> -n mutisaas-production -- /bin/sh
# Try: curl http://sqlserver:1433 (should show network connectivity)
# Try: redis-cli -h redis:6379 ping
```

### **High memory usage?**
```bash
# Increase resource limits in k8s/production/config.yaml
# Update: memory requests/limits
# Redeploy with new limits
```

---

## **Production Checklist - Final**

Before going live:

- [ ] All GitHub Secrets added
- [ ] Kubernetes namespace created
- [ ] Registry secrets configured
- [ ] Application secrets loaded
- [ ] Docker image builds successfully
- [ ] Kubernetes manifests validated (`kubectl apply --dry-run=client -f k8s/production/config.yaml`)
- [ ] Database migrations run
- [ ] Health check endpoint responding
- [ ] Logs flowing into monitoring system
- [ ] Alerts configured
- [ ] Backup strategy in place
- [ ] SSL/TLS certificates installed (if custom domain)
- [ ] DNS records updated (if applicable)
- [ ] Team notified of deployment

---

## **Next Steps After Deployment**

1. **Monitor for 24 hours**
   - Watch error rates
   - Check performance metrics
   - Verify logs are flowing

2. **Gather Metrics**
   - Response times
   - Error rates
   - Resource usage
   - Cache hit ratios

3. **Plan V3 Implementation** (6-8 hours)
   - Feature #12: Background Jobs
   - Feature #13: Domain Events
   - Feature #14: Rate Limiting

---

## **Support & Questions**

For deployment issues:
- Check GitHub Actions logs: `Actions` tab in repository
- Check Kubernetes events: `kubectl describe deployment mutisaas-api -n mutisaas-production`
- Review application logs: `kubectl logs -f deployment/mutisaas-api -n mutisaas-production`

---

**Ready to deploy? Start with Phase 1! 🚀**
