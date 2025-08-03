#!/bin/bash

# Angular Application Deployment Script

set -e

echo "🌐 Deploying Angular Application to S3 + CloudFront"

# Check if AWS CLI is installed
if ! command -v aws &> /dev/null; then
    echo "❌ AWS CLI is not installed. Please install it first."
    exit 1
fi

# Check if Node.js and npm are installed
if ! command -v npm &> /dev/null; then
    echo "❌ Node.js/npm is not installed. Please install it first."
    exit 1
fi

# Set environment
ENVIRONMENT=${1:-dev}

echo "📋 Deployment Configuration:"
echo "   Environment: $ENVIRONMENT"

# Change to Angular project directory
cd ../src/ForecastFlow.Client

# Install dependencies
echo "📦 Installing dependencies..."
npm install

# Build for production
echo "🔨 Building Angular application for production..."
npm run build

# Get S3 bucket name from Terraform output
cd ../../Infrastructure
S3_BUCKET=$(terraform output -raw s3_bucket_name 2>/dev/null || echo "")
CLOUDFRONT_ID=$(terraform output -raw cloudfront_distribution_id 2>/dev/null || echo "")

if [ -z "$S3_BUCKET" ]; then
    echo "❌ Could not get S3 bucket name from Terraform. Please deploy infrastructure first."
    exit 1
fi

echo "📤 Uploading to S3 bucket: $S3_BUCKET"

# Upload files to S3
aws s3 sync ../src/ForecastFlow.Client/dist/ s3://$S3_BUCKET/ --delete

# Invalidate CloudFront cache if ID is available
if [ ! -z "$CLOUDFRONT_ID" ]; then
    echo "🔄 Invalidating CloudFront cache..."
    aws cloudfront create-invalidation --distribution-id $CLOUDFRONT_ID --paths "/*"
fi

echo "✅ Angular application deployed successfully!"

# Get the web app URL
WEB_APP_URL=$(terraform output -raw web_app_url 2>/dev/null || echo "")
if [ ! -z "$WEB_APP_URL" ]; then
    echo "🌐 Web App URL: $WEB_APP_URL"
fi