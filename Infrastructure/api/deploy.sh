#!/bin/bash

# .NET API Deployment Script for ECS

set -e

echo "🚀 Deploying .NET API to ECS"

# Check if AWS CLI is installed
if ! command -v aws &> /dev/null; then
    echo "❌ AWS CLI is not installed. Please install it first."
    exit 1
fi

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install it first."
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
AWS_REGION=${2:-us-east-1}

echo "📋 Deployment Configuration:"
echo "   Environment: $ENVIRONMENT"
echo "   AWS Region: $AWS_REGION"

# Get AWS account ID
AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)

# ECR repository name
ECR_REPO_NAME="${PROJECT_NAME}-${ENVIRONMENT}-api"

# Create ECR repository if it doesn't exist
echo "🏗️ Creating ECR repository if it doesn't exist..."
aws ecr describe-repositories --repository-names $ECR_REPO_NAME --region $AWS_REGION 2>/dev/null || \
aws ecr create-repository --repository-name $ECR_REPO_NAME --region $AWS_REGION

# Get ECR login token
echo "🔐 Logging in to ECR..."
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com

# Change to API project directory
cd ../src/ForecastFlow.Api

# Create Dockerfile if it doesn't exist
if [ ! -f Dockerfile ]; then
    echo "🐳 Creating Dockerfile..."
    cat > Dockerfile << 'EOF'
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ForecastFlow.Api/ForecastFlow.Api.csproj", "ForecastFlow.Api/"]
COPY ["ForecastFlow.Core/ForecastFlow.Core.csproj", "ForecastFlow.Core/"]
RUN dotnet restore "ForecastFlow.Api/ForecastFlow.Api.csproj"
COPY . .
WORKDIR "/src/ForecastFlow.Api"
RUN dotnet build "ForecastFlow.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ForecastFlow.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ForecastFlow.Api.dll"]
EOF
fi

# Build Docker image
echo "🔨 Building Docker image..."
cd ../
docker build -t $ECR_REPO_NAME:latest -f ForecastFlow.Api/Dockerfile .

# Tag image for ECR
docker tag $ECR_REPO_NAME:latest $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$ECR_REPO_NAME:latest

# Push image to ECR
echo "📤 Pushing image to ECR..."
docker push $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$ECR_REPO_NAME:latest

# Update ECS service
cd ../Infrastructure
ECS_CLUSTER_NAME=$(terraform output -raw ecs_cluster_name 2>/dev/null || echo "")
ECS_SERVICE_NAME=$(terraform output -raw ecs_service_name 2>/dev/null || echo "")

if [ -z "$ECS_CLUSTER_NAME" ] || [ -z "$ECS_SERVICE_NAME" ]; then
    echo "❌ Could not get ECS cluster/service names from Terraform. Please deploy infrastructure first."
    exit 1
fi

echo "🔄 Updating ECS service..."
aws ecs update-service \
    --cluster $ECS_CLUSTER_NAME \
    --service $ECS_SERVICE_NAME \
    --force-new-deployment \
    --region $AWS_REGION

echo "⏳ Waiting for service to stabilize..."
aws ecs wait services-stable \
    --cluster $ECS_CLUSTER_NAME \
    --services $ECS_SERVICE_NAME \
    --region $AWS_REGION

echo "✅ .NET API deployed successfully!"

# Get the API endpoint
API_ENDPOINT=$(terraform output -raw api_endpoint 2>/dev/null || echo "")
if [ ! -z "$API_ENDPOINT" ]; then
    echo "🌐 API Endpoint: $API_ENDPOINT"
fi