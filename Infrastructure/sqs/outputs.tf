output "processing_queue_url" {
  description = "URL of the processing queue"
  value       = aws_sqs_queue.processing_queue.id
}

output "processing_queue_arn" {
  description = "ARN of the processing queue"
  value       = aws_sqs_queue.processing_queue.arn
}

output "dlq_url" {
  description = "URL of the dead letter queue"
  value       = aws_sqs_queue.dlq.id
}

output "dlq_arn" {
  description = "ARN of the dead letter queue"
  value       = aws_sqs_queue.dlq.arn
}

output "weather_notifications_queue_url" {
  description = "URL of the weather notifications queue"
  value       = aws_sqs_queue.weather_notifications.id
}

output "weather_notifications_queue_arn" {
  description = "ARN of the weather notifications queue"
  value       = aws_sqs_queue.weather_notifications.arn
}

output "sqs_access_policy_arn" {
  description = "ARN of the SQS access policy"
  value       = aws_iam_policy.sqs_access.arn
}