#!/bin/bash

# Lambda Function Deployment Script

set -e

echo "⚡ Deploying Lambda Function"

# Check if AWS CLI is installed
if ! command -v aws &> /dev/null; then
    echo "❌ AWS CLI is not installed. Please install it first."
    exit 1
fi

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET CLI is not installed. Please install it first."
    exit 1
fi

# Set environment
ENVIRONMENT=${1:-dev}
PROJECT_NAME="forecastflow"

echo "📋 Deployment Configuration:"
echo "   Environment: $ENVIRONMENT"

# Change to Lambda project directory
cd ../src/ForecastFlow.Lambda

# Restore packages
echo "📦 Restoring .NET packages..."
dotnet restore

# Build project
echo "🔨 Building Lambda function..."
dotnet build -c Release

# Publish for deployment
echo "📦 Publishing Lambda function..."
dotnet publish -c Release -o publish

# Create deployment package
echo "📋 Creating deployment package..."
cd publish
zip -r ../lambda-deployment.zip .
cd ..

# Get Lambda function name from Terraform
cd ../../Infrastructure
FUNCTION_NAME=$(terraform output -raw lambda_function_name 2>/dev/null || echo "")

if [ -z "$FUNCTION_NAME" ]; then
    echo "❌ Could not get Lambda function name from Terraform. Please deploy infrastructure first."
    exit 1
fi

echo "⚡ Updating Lambda function: $FUNCTION_NAME"

# Update Lambda function code
aws lambda update-function-code \
    --function-name $FUNCTION_NAME \
    --zip-file fileb://../src/ForecastFlow.Lambda/lambda-deployment.zip

# Wait for update to complete
echo "⏳ Waiting for Lambda function update to complete..."
aws lambda wait function-updated --function-name $FUNCTION_NAME

echo "✅ Lambda function deployed successfully!"

# Clean up
rm -f ../src/ForecastFlow.Lambda/lambda-deployment.zip
rm -rf ../src/ForecastFlow.Lambda/publish