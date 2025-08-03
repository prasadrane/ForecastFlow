# DB Subnet Group
resource "aws_db_subnet_group" "main" {
  name       = "${var.project_name}-${var.environment}-db-subnet-group"
  subnet_ids = var.private_subnet_ids

  tags = merge(var.tags, {
    Name = "${var.project_name}-${var.environment}-db-subnet-group"
  })
}

# Random password for DB
resource "random_password" "db_password" {
  length  = 16
  special = true
}

# Store password in AWS Secrets Manager
resource "aws_secretsmanager_secret" "db_password" {
  name = "${var.project_name}-${var.environment}-db-password"

  tags = var.tags
}

resource "aws_secretsmanager_secret_version" "db_password" {
  secret_id     = aws_secretsmanager_secret.db_password.id
  secret_string = jsonencode({
    username = var.db_username
    password = random_password.db_password.result
  })
}

# RDS SQL Server Instance
resource "aws_db_instance" "main" {
  identifier = "${var.project_name}-${var.environment}-sqlserver"

  # Engine settings
  engine         = "sqlserver-ex"  # SQL Server Express (free tier)
  engine_version = "15.00.4322.2.v1"
  license_model  = "license-included"

  # Instance settings
  instance_class = "db.t3.micro"  # Free tier eligible
  allocated_storage     = 20
  max_allocated_storage = 100
  storage_type          = "gp2"
  storage_encrypted     = true

  # Database settings
  db_name  = "ForecastFlowDB"
  username = var.db_username
  password = random_password.db_password.result

  # Network settings
  db_subnet_group_name   = aws_db_subnet_group.main.name
  vpc_security_group_ids = [var.security_group_id]
  publicly_accessible    = false

  # Backup settings
  backup_retention_period = 7
  backup_window          = "03:00-04:00"
  maintenance_window     = "sun:04:00-sun:05:00"

  # Monitoring
  monitoring_interval = 0  # Disable enhanced monitoring to save cost
  
  # Other settings
  skip_final_snapshot = true
  deletion_protection = false

  tags = merge(var.tags, {
    Name = "${var.project_name}-${var.environment}-sqlserver"
  })
}