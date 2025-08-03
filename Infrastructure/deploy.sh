#!/bin/bash

# ForecastFlow Infrastructure Deployment Script

set -e

echo "🚀 Starting ForecastFlow Infrastructure Deployment"

# Check if AWS CLI is installed
if ! command -v aws &> /dev/null; then
    echo "❌ AWS CLI is not installed. Please install it first."
    exit 1
fi

# Check if Terraform is installed
if ! command -v terraform &> /dev/null; then
    echo "❌ Terraform is not installed. Please install it first."
    exit 1
fi

# Set default values
ENVIRONMENT=${1:-dev}
AWS_REGION=${2:-us-east-1}

echo "📋 Deployment Configuration:"
echo "   Environment: $ENVIRONMENT"
echo "   AWS Region: $AWS_REGION"

# Check AWS credentials
if ! aws sts get-caller-identity &> /dev/null; then
    echo "❌ AWS credentials not configured. Please run 'aws configure' first."
    exit 1
fi

# Change to Infrastructure directory
cd Infrastructure

# Initialize Terraform
echo "🔧 Initializing Terraform..."
terraform init

# Plan deployment
echo "📝 Planning infrastructure deployment..."
terraform plan \
    -var="environment=$ENVIRONMENT" \
    -var="aws_region=$AWS_REGION" \
    -out=tfplan

# Ask for confirmation
echo ""
read -p "Do you want to apply this plan? (y/N): " -n 1 -r
echo ""

if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "🚀 Applying infrastructure..."
    terraform apply tfplan
    
    echo "✅ Infrastructure deployment completed!"
    echo ""
    echo "📋 Deployment Outputs:"
    terraform output
    
    echo ""
    echo "🔗 Next Steps:"
    echo "1. Build and deploy the Angular application to S3"
    echo "2. Build and deploy the .NET API to ECS"
    echo "3. Build and deploy the Lambda function"
    echo ""
    echo "Use the deployment scripts in the respective folders for application deployments."
else
    echo "❌ Deployment cancelled."
    rm -f tfplan
fi