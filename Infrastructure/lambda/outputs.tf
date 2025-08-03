output "function_name" {
  description = "Name of the Lambda function"
  value       = aws_lambda_function.weather_processing.function_name
}

output "function_arn" {
  description = "ARN of the Lambda function"
  value       = aws_lambda_function.weather_processing.arn
}

output "lambda_role_arn" {
  description = "ARN of the Lambda IAM role"
  value       = aws_iam_role.lambda_role.arn
}