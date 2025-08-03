# SQS Queue for processing tasks
resource "aws_sqs_queue" "processing_queue" {
  name                       = "${var.project_name}-${var.environment}-processing-queue"
  delay_seconds             = 0
  max_message_size          = 262144
  message_retention_seconds = 1209600  # 14 days
  receive_wait_time_seconds = 20       # Long polling
  visibility_timeout_seconds = 300     # 5 minutes

  # Redrive policy for failed messages
  redrive_policy = jsonencode({
    deadLetterTargetArn = aws_sqs_queue.dlq.arn
    maxReceiveCount     = 3
  })

  tags = merge(var.tags, {
    Name = "${var.project_name}-${var.environment}-processing-queue"
  })
}

# Dead Letter Queue
resource "aws_sqs_queue" "dlq" {
  name                       = "${var.project_name}-${var.environment}-processing-dlq"
  message_retention_seconds = 1209600  # 14 days

  tags = merge(var.tags, {
    Name = "${var.project_name}-${var.environment}-processing-dlq"
  })
}

# SQS Queue for weather notifications
resource "aws_sqs_queue" "weather_notifications" {
  name                       = "${var.project_name}-${var.environment}-weather-notifications"
  delay_seconds             = 0
  max_message_size          = 262144
  message_retention_seconds = 432000   # 5 days
  receive_wait_time_seconds = 20       # Long polling
  visibility_timeout_seconds = 60      # 1 minute

  tags = merge(var.tags, {
    Name = "${var.project_name}-${var.environment}-weather-notifications"
  })
}

# IAM policy for SQS access
resource "aws_iam_policy" "sqs_access" {
  name        = "${var.project_name}-${var.environment}-sqs-access"
  description = "Policy for accessing SQS queues"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "sqs:SendMessage",
          "sqs:ReceiveMessage",
          "sqs:DeleteMessage",
          "sqs:GetQueueAttributes",
          "sqs:GetQueueUrl"
        ]
        Resource = [
          aws_sqs_queue.processing_queue.arn,
          aws_sqs_queue.dlq.arn,
          aws_sqs_queue.weather_notifications.arn
        ]
      }
    ]
  })

  tags = var.tags
}