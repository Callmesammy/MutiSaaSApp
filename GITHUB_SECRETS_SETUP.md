# 📋 GitHub Secrets Configuration

> Quick reference for setting up GitHub Secrets for CI/CD pipeline

---

## **Where to Add Secrets**

1. Go to your repository: https://github.com/Callmesammy/MutiSaaSApp
2. Click **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Add each secret below

---

## **Required Secrets**

### **1. SONAR_TOKEN** (Code Quality)
**Purpose:** SonarCloud code quality analysis

**How to get:**
1. Visit: https://sonarcloud.io/account/security/
2. Click "Generate Tokens"
3. Give it a name (e.g., "GitHub Actions")
4. Copy the token

**Add to GitHub:**
- Secret name: `SONAR_TOKEN`
- Secret value: `squ_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx`

---

### **2. KUBECONFIG** (Kubernetes Deployment)
**Purpose:** Deploy to Kubernetes cluster

**How to get:**
1. Get your kubeconfig file from cluster admin or:
   ```bash
   # If you have kubectl configured locally
   cat ~/.kube/config
   ```
2. **IMPORTANT:** Encode it to base64:
   ```bash
   # macOS/Linux
   cat ~/.kube/config | base64 -w 0
   
   # Windows PowerShell
   [Convert]::ToBase64String([IO.File]::ReadAllBytes("$env:USERPROFILE\.kube\config")) | Set-Clipboard
   ```
3. Copy the entire base64 string

**Add to GitHub:**
- Secret name: `KUBECONFIG`
- Secret value: `YXBpVmVyc2lvbjogdjEKY2x1c3RlcnM6...` (base64 encoded full config)

---

### **3. GHCR_TOKEN** (Container Registry)
**Purpose:** Push Docker images to GitHub Container Registry

**How to get:**
1. Go to GitHub → Settings → **Developer settings** → **Personal access tokens** → **Tokens (classic)**
2. Click **"Generate new token (classic)"**
3. Name: `MutiSaaS CI/CD`
4. Select scopes:
   - ✓ `write:packages`
   - ✓ `read:packages`
   - ✓ `delete:packages` (optional, for cleanup)
5. Click **Generate token**
6. Copy the token (you won't see it again!)

**Add to GitHub:**
- Secret name: `GHCR_TOKEN`
- Secret value: `ghp_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx`

---

### **4. GHCR_USERNAME** (Container Registry Username)
**Purpose:** Identify who's pushing Docker images

**Value:** Your GitHub username (e.g., `callmesammy`)

**Add to GitHub:**
- Secret name: `GHCR_USERNAME`
- Secret value: `callmesammy`

---

## **Optional Secrets** (Recommended for Production)

### **5. SONARCLOUD_ORGANIZATION** (SonarCloud Org Key)
**Purpose:** Specify which SonarCloud organization to use

**How to get:**
- Visit: https://sonarcloud.io/organizations
- Copy organization key (e.g., `callmesammy-github`)

**Add to GitHub:**
- Secret name: `SONARCLOUD_ORGANIZATION`
- Secret value: `callmesammy-github`

---

### **6. DOCKER_REGISTRY_URL** (Container Registry URL)
**Purpose:** Configure custom registry (optional, defaults to GHCR)

**Value:** `ghcr.io`

**Add to GitHub:**
- Secret name: `DOCKER_REGISTRY_URL`
- Secret value: `ghcr.io`

---

## **Setup Checklist**

```markdown
GitHub Secrets Setup Checklist
==============================

□ SONAR_TOKEN
  - From: https://sonarcloud.io/account/security/
  - Value: squ_... (40+ characters)
  - Verified: ✓ (Try building, SonarCloud analysis will confirm)

□ KUBECONFIG
  - From: Kubernetes cluster admin
  - Value: base64 encoded kubeconfig
  - Encoded: ✓ (Must be base64!)
  - Verified: ✓ (Deployment logs will confirm connection)

□ GHCR_TOKEN
  - From: GitHub → Developer settings → Tokens
  - Value: ghp_... (60+ characters)
  - Scopes: write:packages, read:packages
  - Verified: ✓ (Docker push will confirm)

□ GHCR_USERNAME
  - Value: Your GitHub username
  - Verified: ✓ (Docker image tags will show it)

□ SONARCLOUD_ORGANIZATION (optional)
  - From: https://sonarcloud.io/organizations
  - Value: organization-key

□ DOCKER_REGISTRY_URL (optional)
  - Value: ghcr.io
```

---

## **Verification Commands**

After adding secrets, verify they work:

### **SONAR_TOKEN Check**
```bash
# Will see SonarCloud analysis in GitHub Actions logs
# Look for: "SonarCloud analysis started"
```

### **KUBECONFIG Check**
```bash
# After adding, decode to verify format
echo "your_base64_value" | base64 -d | head -20
# Should show YAML starting with "apiVersion: v1"
```

### **GHCR_TOKEN Check**
```bash
# Will see Docker image pushed to GHCR
# Look for: "Pushing image to ghcr.io/callmesammy/mutisaasapp"
```

---

## **Troubleshooting**

### **"Secret not found" in Actions**
- Ensure secret name matches exactly (case-sensitive)
- Refresh your browser
- Wait 1 minute after adding

### **"Failed to authenticate with registry"**
- Verify GHCR_TOKEN hasn't expired
- Check GHCR_USERNAME is correct
- Token must have `write:packages` scope

### **"Invalid kubeconfig"**
- Ensure it's base64 encoded
- Verify it decodes to valid YAML
- Check cluster credentials haven't rotated

### **"Deployment failed" in Actions**
- Check KUBECONFIG is current
- Verify Kubernetes cluster is accessible
- Check namespace exists: `kubectl get namespace mutisaas-production`

---

## **Quick Setup Command**

If you have all values ready, here's a one-liner to add all secrets:

```bash
# Add each secret (requires GitHub CLI: https://cli.github.com)
gh secret set SONAR_TOKEN --body "your_token" -R Callmesammy/MutiSaaSApp
gh secret set KUBECONFIG --body "$(cat ~/.kube/config | base64 -w 0)" -R Callmesammy/MutiSaaSApp
gh secret set GHCR_TOKEN --body "your_token" -R Callmesammy/MutiSaaSApp
gh secret set GHCR_USERNAME --body "your_username" -R Callmesammy/MutiSaaSApp
```

---

## **Next Steps**

After secrets are configured:

1. **Push code to master:**
   ```bash
   git add .
   git commit -m "Ready for production deployment"
   git push origin master
   ```

2. **Watch GitHub Actions:**
   - Go to: `Actions` tab in your repository
   - Watch build progress
   - Check for deployment approval

3. **Approve deployment:**
   - When prompted, approve production deployment
   - Workflow will deploy to Kubernetes

4. **Verify deployment:**
   ```bash
   kubectl get pods -n mutisaas-production
   kubectl logs -f deployment/mutisaas-api -n mutisaas-production
   ```

---

**All secrets configured? → Ready to deploy! 🚀**
