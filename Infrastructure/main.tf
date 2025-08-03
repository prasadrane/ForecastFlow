terraform {
  required_version = ">= 1.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region = var.aws_region

  default_tags {
    tags = {
      Project     = "ForecastFlow"
      Environment = var.environment
      ManagedBy   = "Terraform"
    }
  }
}

# Data sources
data "aws_availability_zones" "available" {
  state = "available"
}

data "aws_caller_identity" "current" {}

# Local values
locals {
  project_name = "forecastflow"
  common_tags = {
    Project     = "ForecastFlow"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# Modules
module "shared" {
  source = "./shared"
  
  project_name = local.project_name
  environment  = var.environment
  aws_region   = var.aws_region
  
  availability_zones = data.aws_availability_zones.available.names
  
  tags = local.common_tags
}

module "sql_server" {
  source = "./sql-server"
  
  project_name = local.project_name
  environment  = var.environment
  
  vpc_id            = module.shared.vpc_id
  private_subnet_ids = module.shared.private_subnet_ids
  security_group_id = module.shared.rds_security_group_id
  
  tags = local.common_tags
}

module "sqs" {
  source = "./sqs"
  
  project_name = local.project_name
  environment  = var.environment
  
  tags = local.common_tags
}

module "lambda" {
  source = "./lambda"
  
  project_name = local.project_name
  environment  = var.environment
  
  vpc_id                = module.shared.vpc_id
  private_subnet_ids    = module.shared.private_subnet_ids
  lambda_security_group_id = module.shared.lambda_security_group_id
  
  sqs_queue_url = module.sqs.processing_queue_url
  sqs_queue_arn = module.sqs.processing_queue_arn
  
  rds_endpoint = module.sql_server.rds_endpoint
  
  tags = local.common_tags
}

module "api" {
  source = "./api"
  
  project_name = local.project_name
  environment  = var.environment
  
  vpc_id                = module.shared.vpc_id
  public_subnet_ids     = module.shared.public_subnet_ids
  private_subnet_ids    = module.shared.private_subnet_ids
  alb_security_group_id = module.shared.alb_security_group_id
  ecs_security_group_id = module.shared.ecs_security_group_id
  
  rds_endpoint  = module.sql_server.rds_endpoint
  sqs_queue_url = module.sqs.processing_queue_url
  
  tags = local.common_tags
}

module "angular" {
  source = "./angular"
  
  project_name = local.project_name
  environment  = var.environment
  
  api_domain_name = module.api.api_domain_name
  
  tags = local.common_tags
}