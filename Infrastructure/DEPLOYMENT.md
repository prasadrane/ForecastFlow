# ForecastFlow AWS Infrastructure Deployment Guide

This guide walks you through deploying the complete ForecastFlow application to AWS using Terraform Infrastructure as Code.

## 🏗️ Infrastructure Overview

The AWS infrastructure includes:

### **Public Components** (Internet Accessible)
- **Angular Web App**: S3 + CloudFront for global CDN
- **.NET Web API**: ECS Fargate + Application Load Balancer

### **Private Components** (VPC Internal Only)
- **AWS Lambda**: Serverless weather processing functions
- **AWS SQS**: Message queues for async processing
- **SQL Server**: RDS SQL Server Express for data persistence

### **Networking & Security**
- **VPC**: Isolated network with public/private subnets
- **NAT Gateways**: Outbound internet access for private resources
- **Security Groups**: Restrictive firewall rules
- **IAM Roles**: Least-privilege access policies

## 📋 Prerequisites

Before deploying, ensure you have:

### **Required Tools**
- [AWS CLI](https://aws.amazon.com/cli/) v2.0+
- [Terraform](https://terraform.io/) v1.0+
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) and npm
- [Docker](https://docker.com/) (for API deployment)

### **AWS Account Setup**
```bash
# Configure AWS credentials
aws configure

# Verify access
aws sts get-caller-identity
```

### **Cost Considerations**
This infrastructure uses AWS Free Tier eligible services where possible:
- **RDS SQL Server Express**: t3.micro instance (~$13/month)
- **ECS Fargate**: 0.25 vCPU, 0.5GB RAM (~$5/month)
- **NAT Gateways**: ~$32/month (required for private resources)
- **S3 + CloudFront**: Minimal cost for static hosting
- **Lambda + SQS**: Pay-per-use, minimal cost for low traffic

**Estimated monthly cost: ~$50/month**

## 🚀 Deployment Steps

### **1. Clone and Setup**

```bash
git clone <repository-url>
cd ForecastFlow
```

### **2. Configure Variables**

```bash
cd Infrastructure
cp terraform.tfvars.example terraform.tfvars
```

Edit `terraform.tfvars`:
```hcl
aws_region  = "us-east-1"
environment = "dev"
db_username = "forecastflow_admin"
db_password = "YourSecurePasswordHere123!"  # Change this!
```

### **3. Deploy Infrastructure**

```bash
# Run the deployment script
./deploy.sh dev us-east-1

# Or manually:
terraform init
terraform plan
terraform apply
```

**This takes ~15-20 minutes** to provision all AWS resources.

### **4. Deploy Applications**

#### **Deploy Lambda Function**
```bash
cd lambda
./deploy.sh dev
```

#### **Deploy .NET API**
```bash
cd ../api
./deploy.sh dev us-east-1
```

#### **Deploy Angular Web App**
```bash
cd ../angular
./deploy.sh dev
```

### **5. Verify Deployment**

Get the deployed URLs:
```bash
cd ../
terraform output
```

Example output:
```
api_endpoint = "http://forecastflow-dev-alb-123456789.us-east-1.elb.amazonaws.com"
web_app_url = "https://d123456789.cloudfront.net"
```

## 🔧 Post-Deployment Configuration

### **Database Setup**
The RDS instance is created with:
- **Database**: ForecastFlowDB
- **Username**: forecastflow_admin
- **Password**: Stored in AWS Secrets Manager

Run Entity Framework migrations:
```bash
cd ../src/ForecastFlow.Api

# Update connection string in appsettings.json with RDS endpoint
# Run migrations
dotnet ef database update
```

### **Environment Variables**
The following environment variables are automatically configured:
- `ConnectionStrings__DefaultConnection`: RDS connection string
- `AWS__SQS__QueueUrl`: SQS queue URL
- `ASPNETCORE_ENVIRONMENT`: Environment name

### **API Configuration**
Update the Angular app to use the deployed API endpoint:
```typescript
// src/environments/environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'http://your-alb-endpoint.elb.amazonaws.com/api'
};
```

## 🏃 Testing the Deployment

### **Health Checks**
```bash
# Check API health
curl http://your-alb-endpoint/health

# Check Lambda function
aws lambda invoke --function-name forecastflow-dev-weather-processing \
  --payload '{"test": "message"}' response.json
```

### **Send Test Message**
```bash
# Send message to SQS queue
aws sqs send-message \
  --queue-url https://sqs.us-east-1.amazonaws.com/123456789/forecastflow-dev-processing-queue \
  --message-body '{"MessageType":"WeatherUpdate","Location":"New York"}'
```

## 🔄 Updating Deployments

### **Update Infrastructure**
```bash
cd Infrastructure
terraform plan
terraform apply
```

### **Update Applications**
```bash
# Update Lambda
cd lambda && ./deploy.sh dev

# Update API
cd ../api && ./deploy.sh dev

# Update Web App
cd ../angular && ./deploy.sh dev
```

## 🧹 Cleanup

To delete all resources and stop billing:
```bash
cd Infrastructure
terraform destroy
```

**Warning**: This will permanently delete all data!

## 🐛 Troubleshooting

### **Common Issues**

**Lambda Deployment Fails**
```bash
# Check function logs
aws logs tail /aws/lambda/forecastflow-dev-weather-processing --follow
```

**ECS Service Won't Start**
```bash
# Check service events
aws ecs describe-services --cluster forecastflow-dev-cluster --services forecastflow-dev-api-service
```

**RDS Connection Issues**
- Verify security groups allow connections from ECS and Lambda
- Check that RDS is in the same VPC as the application components
- Ensure connection string includes the correct endpoint and credentials

### **Monitoring and Logs**

**CloudWatch Logs**
- Lambda: `/aws/lambda/forecastflow-dev-weather-processing`
- ECS: `/ecs/forecastflow-dev-api`

**ECS Service Monitoring**
```bash
aws ecs describe-services --cluster forecastflow-dev-cluster --services forecastflow-dev-api-service
```

## 📚 Architecture Details

### **Network Architecture**
```
Internet Gateway
    │
    ├── Public Subnet 1A (ALB, NAT Gateway)
    ├── Public Subnet 1B (ALB, NAT Gateway)
    │
    ├── Private Subnet 1A (ECS, Lambda)
    ├── Private Subnet 1B (ECS, Lambda, RDS)
```

### **Security Groups**
- **ALB**: Allows HTTP/HTTPS from internet
- **ECS**: Allows HTTP from ALB only
- **Lambda**: Outbound only for API calls
- **RDS**: Allows SQL Server connections from ECS/Lambda only

### **IAM Roles**
- **ECS Task Role**: SQS access, Secrets Manager access
- **Lambda Role**: SQS access, VPC access, CloudWatch Logs
- **ECS Execution Role**: ECR access, CloudWatch Logs

This infrastructure provides a production-ready, scalable, and secure foundation for the ForecastFlow application.