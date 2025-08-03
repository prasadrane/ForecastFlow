# Lambda IAM Role
resource "aws_iam_role" "lambda_role" {
  name = "${var.project_name}-${var.environment}-lambda-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "lambda.amazonaws.com"
        }
      }
    ]
  })

  tags = var.tags
}

# Lambda IAM Policy
resource "aws_iam_policy" "lambda_policy" {
  name        = "${var.project_name}-${var.environment}-lambda-policy"
  description = "Policy for Lambda function"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "logs:CreateLogGroup",
          "logs:CreateLogStream",
          "logs:PutLogEvents"
        ]
        Resource = "arn:aws:logs:*:*:*"
      },
      {
        Effect = "Allow"
        Action = [
          "ec2:CreateNetworkInterface",
          "ec2:DescribeNetworkInterfaces",
          "ec2:DeleteNetworkInterface"
        ]
        Resource = "*"
      },
      {
        Effect = "Allow"
        Action = [
          "sqs:ReceiveMessage",
          "sqs:DeleteMessage",
          "sqs:GetQueueAttributes",
          "sqs:SendMessage"
        ]
        Resource = [
          var.sqs_queue_arn,
          "${var.sqs_queue_arn}-dlq"
        ]
      },
      {
        Effect = "Allow"
        Action = [
          "secretsmanager:GetSecretValue"
        ]
        Resource = "*"
      }
    ]
  })

  tags = var.tags
}

# Attach policy to role
resource "aws_iam_role_policy_attachment" "lambda_policy" {
  role       = aws_iam_role.lambda_role.name
  policy_arn = aws_iam_policy.lambda_policy.arn
}

# Lambda function placeholder (will be updated with actual deployment)
resource "aws_lambda_function" "weather_processing" {
  filename         = "placeholder.zip"
  function_name    = "${var.project_name}-${var.environment}-weather-processing"
  role            = aws_iam_role.lambda_role.arn
  handler         = "ForecastFlow.Lambda::ForecastFlow.Lambda.WeatherProcessingFunction::FunctionHandler"
  runtime         = "dotnet8"
  timeout         = 300
  memory_size     = 256

  vpc_config {
    subnet_ids         = var.private_subnet_ids
    security_group_ids = [var.lambda_security_group_id]
  }

  environment {
    variables = {
      SQS_QUEUE_URL = var.sqs_queue_url
      RDS_ENDPOINT  = var.rds_endpoint
      ENVIRONMENT   = var.environment
    }
  }

  depends_on = [
    aws_iam_role_policy_attachment.lambda_policy,
    aws_cloudwatch_log_group.lambda_logs
  ]

  tags = var.tags

  lifecycle {
    ignore_changes = [filename, source_code_hash]
  }
}

# CloudWatch Log Group
resource "aws_cloudwatch_log_group" "lambda_logs" {
  name              = "/aws/lambda/${var.project_name}-${var.environment}-weather-processing"
  retention_in_days = 14

  tags = var.tags
}

# SQS Event Source Mapping
resource "aws_lambda_event_source_mapping" "sqs_trigger" {
  event_source_arn = var.sqs_queue_arn
  function_name    = aws_lambda_function.weather_processing.arn
  batch_size       = 10
  
  depends_on = [aws_lambda_function.weather_processing]
}

# Create placeholder zip file for initial deployment
data "archive_file" "placeholder" {
  type        = "zip"
  output_path = "placeholder.zip"
  
  source {
    content  = "placeholder"
    filename = "placeholder.txt"
  }
}