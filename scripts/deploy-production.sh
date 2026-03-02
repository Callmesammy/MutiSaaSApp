#!/bin/bash
# Deploy to production environment
# Usage: ./deploy-production.sh <image-tag>

set -e

IMAGE_TAG=${1:-latest}
NAMESPACE=mutisaas-production
DEPLOYMENT_NAME=mutisaas-api

echo "🚀 Deploying to production environment..."
echo "Image tag: $IMAGE_TAG"
echo "Namespace: $NAMESPACE"

# Create namespace if it doesn't exist
kubectl create namespace $NAMESPACE --dry-run=client -o yaml | kubectl apply -f -

# Apply production configurations
kubectl apply -f k8s/production/config.yaml --namespace=$NAMESPACE

# Update deployment image
kubectl set image deployment/$DEPLOYMENT_NAME \
  mutisaas-api=ghcr.io/callmesammy/mutisaasapp:$IMAGE_TAG \
  --namespace=$NAMESPACE

# Wait for rollout
kubectl rollout status deployment/$DEPLOYMENT_NAME --namespace=$NAMESPACE

echo "✅ Production deployment completed successfully!"
