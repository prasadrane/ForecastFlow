output "vpc_id" {
  description = "ID of the VPC"
  value       = module.shared.vpc_id
}

output "api_endpoint" {
  description = "API endpoint URL"
  value       = module.api.api_endpoint
}

output "web_app_url" {
  description = "Web application URL"
  value       = module.angular.web_app_url
}

output "database_endpoint" {
  description = "RDS database endpoint"
  value       = module.sql_server.rds_endpoint
  sensitive   = true
}

output "sqs_queue_url" {
  description = "SQS queue URL"
  value       = module.sqs.processing_queue_url
  sensitive   = true
}

output "lambda_function_name" {
  description = "Lambda function name"
  value       = module.lambda.function_name
}