#!/bin/bash

# Production Deployment Setup Script
# This script automates the Kubernetes cluster setup for production deployment

set -e  # Exit on error

echo "🚀 MutiSaaS App - Production Deployment Setup"
echo "=============================================="
echo ""

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Check prerequisites
check_prerequisites() {
    echo -e "${BLUE}[1/5] Checking prerequisites...${NC}"
    
    if ! command -v kubectl &> /dev/null; then
        echo -e "${RED}✗ kubectl not found. Please install kubectl.${NC}"
        exit 1
    fi
    
    if ! command -v base64 &> /dev/null; then
        echo -e "${RED}✗ base64 not found.${NC}"
        exit 1
    fi
    
    # Check if connected to cluster
    if ! kubectl cluster-info &> /dev/null; then
        echo -e "${RED}✗ Not connected to Kubernetes cluster. Please configure kubeconfig.${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✓ All prerequisites met${NC}"
    echo ""
}

# Create namespace
create_namespace() {
    echo -e "${BLUE}[2/5] Creating namespace...${NC}"
    
    if kubectl get namespace mutisaas-production &> /dev/null; then
        echo -e "${YELLOW}⚠ Namespace 'mutisaas-production' already exists${NC}"
    else
        kubectl create namespace mutisaas-production
        echo -e "${GREEN}✓ Namespace created${NC}"
    fi
    echo ""
}

# Create Docker registry secret
create_registry_secret() {
    echo -e "${BLUE}[3/5] Creating Docker registry secret...${NC}"
    
    read -p "Enter GitHub username: " GITHUB_USERNAME
    read -sp "Enter GitHub Container Registry token: " GHCR_TOKEN
    echo ""
    read -p "Enter your email: " EMAIL
    
    # Check if secret already exists
    if kubectl get secret ghcr-secret -n mutisaas-production &> /dev/null; then
        echo -e "${YELLOW}⚠ Secret 'ghcr-secret' already exists. Deleting...${NC}"
        kubectl delete secret ghcr-secret -n mutisaas-production
    fi
    
    kubectl create secret docker-registry ghcr-secret \
        --docker-server=ghcr.io \
        --docker-username="$GITHUB_USERNAME" \
        --docker-password="$GHCR_TOKEN" \
        --docker-email="$EMAIL" \
        -n mutisaas-production
    
    echo -e "${GREEN}✓ Docker registry secret created${NC}"
    echo ""
}

# Create application secrets
create_app_secrets() {
    echo -e "${BLUE}[4/5] Creating application secrets...${NC}"
    
    read -p "Enter SQL Server connection string: " DB_CONNECTION
    read -p "Enter Redis connection string: " CACHE_CONNECTION
    read -sp "Enter JWT Secret (32+ chars): " JWT_SECRET
    echo ""
    
    # Check if secret already exists
    if kubectl get secret app-secrets -n mutisaas-production &> /dev/null; then
        echo -e "${YELLOW}⚠ Secret 'app-secrets' already exists. Deleting...${NC}"
        kubectl delete secret app-secrets -n mutisaas-production
    fi
    
    kubectl create secret generic app-secrets \
        --from-literal=DatabaseConnection="$DB_CONNECTION" \
        --from-literal=CacheConnection="$CACHE_CONNECTION" \
        --from-literal=JwtSecret="$JWT_SECRET" \
        -n mutisaas-production
    
    echo -e "${GREEN}✓ Application secrets created${NC}"
    echo ""
}

# Validate Kubernetes manifests
validate_manifests() {
    echo -e "${BLUE}[5/5] Validating Kubernetes manifests...${NC}"
    
    if [ -f "k8s/production/config.yaml" ]; then
        if kubectl apply --dry-run=client -f k8s/production/config.yaml &> /dev/null; then
            echo -e "${GREEN}✓ Manifests are valid${NC}"
        else
            echo -e "${RED}✗ Manifest validation failed${NC}"
            exit 1
        fi
    else
        echo -e "${YELLOW}⚠ k8s/production/config.yaml not found${NC}"
    fi
    echo ""
}

# Summary
print_summary() {
    echo -e "${GREEN}========================================${NC}"
    echo -e "${GREEN}✓ Production cluster setup complete!${NC}"
    echo -e "${GREEN}========================================${NC}"
    echo ""
    echo "Next steps:"
    echo "1. Push code to master branch:"
    echo "   git add ."
    echo "   git commit -m 'Deploy to production'"
    echo "   git push origin master"
    echo ""
    echo "2. GitHub Actions workflow will:"
    echo "   - Build and test (41 tests)"
    echo "   - Build Docker image"
    echo "   - Push to GHCR"
    echo "   - Require approval for deployment"
    echo ""
    echo "3. After approval, verify deployment:"
    echo "   kubectl get pods -n mutisaas-production"
    echo "   kubectl logs -f deployment/mutisaas-api -n mutisaas-production"
    echo ""
    echo "4. Test health endpoint:"
    echo "   kubectl port-forward svc/mutisaas-api 8080:80 -n mutisaas-production"
    echo "   curl http://localhost:8080/api/health"
    echo ""
}

# Main execution
main() {
    check_prerequisites
    create_namespace
    create_registry_secret
    create_app_secrets
    validate_manifests
    print_summary
}

# Run main function
main
