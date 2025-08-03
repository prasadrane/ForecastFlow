# ForecastFlow Infrastructure

This folder contains Terraform Infrastructure as Code for deploying ForecastFlow to AWS.

## Architecture Overview

The infrastructure is designed with public and private components:

### Public Components
- **Angular Web App**: Hosted on S3 + CloudFront for global distribution
- **.NET API**: Hosted on ECS Fargate for scalability and cost-effectiveness

### Private Components  
- **AWS Lambda**: Serverless functions for background processing
- **AWS SQS**: Message queues for asynchronous communication
- **SQL Server**: RDS SQL Server for data persistence

## Folder Structure

- `angular/` - S3 and CloudFront configuration for Angular SPA
- `api/` - ECS and ALB configuration for .NET Web API
- `lambda/` - Lambda functions and related resources
- `sqs/` - SQS queues configuration
- `sql-server/` - RDS SQL Server configuration
- `shared/` - Common resources (VPC, IAM, etc.)

## Deployment

```bash
# Initialize Terraform
terraform init

# Plan deployment
terraform plan

# Apply infrastructure
terraform apply
```

## Cost Optimization

- Uses AWS Free Tier eligible services where possible
- RDS SQL Server uses t3.micro instance
- ECS Fargate uses minimum CPU/memory allocation
- S3 and CloudFront for efficient static content delivery