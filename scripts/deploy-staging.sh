#!/bin/bash
# Deploy to staging environment
# Usage: ./deploy-staging.sh <image-tag>

set -e

IMAGE_TAG=${1:-latest}
NAMESPACE=mutisaas-staging
DEPLOYMENT_NAME=mutisaas-api

echo "🚀 Deploying to staging environment..."
echo "Image tag: $IMAGE_TAG"
echo "Namespace: $NAMESPACE"

# Create namespace if it doesn't exist
kubectl create namespace $NAMESPACE --dry-run=client -o yaml | kubectl apply -f -

# Apply staging configurations
kubectl apply -f k8s/staging/config.yaml --namespace=$NAMESPACE

# Update deployment image
kubectl set image deployment/$DEPLOYMENT_NAME \
  mutisaas-api=ghcr.io/callmesammy/mutisaasapp:$IMAGE_TAG \
  --namespace=$NAMESPACE

# Wait for rollout
kubectl rollout status deployment/$DEPLOYMENT_NAME --namespace=$NAMESPACE

echo "✅ Staging deployment completed successfully!"
