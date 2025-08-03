output "web_app_url" {
  description = "CloudFront distribution URL"
  value       = "https://${aws_cloudfront_distribution.web_app.domain_name}"
}

output "s3_bucket_name" {
  description = "Name of the S3 bucket"
  value       = aws_s3_bucket.web_app.bucket
}

output "cloudfront_distribution_id" {
  description = "CloudFront distribution ID"
  value       = aws_cloudfront_distribution.web_app.id
}