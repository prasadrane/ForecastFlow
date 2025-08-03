# Getting Started with ForecastFlow

This guide provides detailed step-by-step instructions for setting up, building, and deploying the ForecastFlow application locally and to AWS.

## 📋 Prerequisites

Before you begin, ensure you have the following installed on your system:

### Required Software
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher
- [Node.js 20+](https://nodejs.org/) and npm
- [SQL Server](https://www.microsoft.com/en-us/sql-server) (LocalDB or full instance)
- [Git](https://git-scm.com/) for version control

### For AWS Deployment (Optional)
- [AWS CLI](https://aws.amazon.com/cli/) configured with appropriate credentials
- [Terraform](https://terraform.io/) for infrastructure management
- AWS Account with appropriate permissions

### Development Tools (Recommended)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/) (SSMS)
- [Postman](https://www.postman.com/) for API testing

## 🚀 Local Development Setup

### Step 1: Clone the Repository

```bash
git clone https://github.com/prasadrane/ForecastFlow.git
cd ForecastFlow
```

### Step 2: Backend Setup (.NET Web API)

#### 2.1 Restore NuGet Packages
```bash
cd src
dotnet restore
```

#### 2.2 Configure Database Connection

**Option A: Using LocalDB (Recommended for Development)**
```json
// File: src/ForecastFlow.Api/appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ForecastFlowDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

**Option B: Using SQL Server Instance**
```json
// File: src/ForecastFlow.Api/appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ForecastFlowDb;Integrated Security=true;TrustServerCertificate=true"
  }
}
```

**Option C: Using SQL Server with Authentication**
```json
// File: src/ForecastFlow.Api/appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ForecastFlowDb;User Id=your_username;Password=your_password;TrustServerCertificate=true"
  }
}
```

#### 2.3 Configure JWT Settings

Create or update the JWT configuration in `appsettings.json`:
```json
{
  "Jwt": {
    "Key": "ThisIsAVeryLongSecretKeyForJWTTokenGenerationThatIsAtLeast256BitsLong",
    "Issuer": "ForecastFlow",
    "Audience": "ForecastFlow",
    "ExpiresInMinutes": "60"
  }
}
```

For production, use User Secrets or environment variables:
```bash
cd src/ForecastFlow.Api
dotnet user-secrets set "Jwt:Key" "your-super-secret-jwt-key-here"
```

#### 2.4 Set Up OpenWeatherMap API Key

1. Create a free account at [OpenWeatherMap](https://openweathermap.org/api)
2. Get your API key
3. Store it securely:

```bash
cd src/ForecastFlow.Api
dotnet user-secrets set "OpenWeatherMap:ApiKey" "your-api-key-here"
```

Or add it to `appsettings.Development.json`:
```json
{
  "OpenWeatherMap": {
    "ApiKey": "your-api-key-here"
  }
}
```

#### 2.5 Create and Run Database Migrations

```bash
cd src/ForecastFlow.Api

# Install Entity Framework Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

#### 2.6 Build and Run the API

```bash
# Build the solution
cd src
dotnet build

# Run the API
cd ForecastFlow.Api
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:7089`
- HTTP: `http://localhost:5089`
- Swagger UI: `https://localhost:7089/swagger`

### Step 3: Frontend Setup (Angular)

#### 3.1 Navigate to Client Directory
```bash
cd src/ForecastFlow.Client
```

#### 3.2 Install Dependencies
```bash
npm install
```

If you encounter dependency conflicts, try:
```bash
npm install --legacy-peer-deps
```

#### 3.3 Configure API Base URL

Update the environment configuration if needed:
```typescript
// File: src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7089/api'
};
```

#### 3.4 Start the Development Server
```bash
npm start
```

The Angular application will be available at `http://localhost:4200`

## 🧪 Running Tests

### Backend Tests
```bash
cd src
dotnet test
```

### Frontend Tests
```bash
cd src/ForecastFlow.Client
npm test
```

For continuous testing:
```bash
npm test -- --watch
```

## 🏗️ Building for Production

### Backend Production Build
```bash
cd src
dotnet build -c Release
dotnet publish -c Release -o ./publish
```

### Frontend Production Build
```bash
cd src/ForecastFlow.Client
npm run build
```

The built files will be in the `dist/` directory.

## 🚀 AWS Deployment

### Prerequisites for AWS Deployment

1. **AWS CLI Configuration**
   ```bash
   aws configure
   # Enter your AWS Access Key ID, Secret Access Key, region, and output format
   ```

2. **Terraform Installation**
   - Download from [terraform.io](https://www.terraform.io/downloads.html)
   - Add to your PATH

3. **Required AWS Permissions**
   Your AWS user/role needs permissions for:
   - S3 (for static website hosting)
   - CloudFront (for CDN)
   - ECS (for API hosting)
   - RDS (for database)
   - Lambda (for serverless functions)
   - SQS (for messaging)
   - Secrets Manager (for secure configuration)
   - IAM (for role management)

### Step-by-Step AWS Deployment

#### 1. Configure Terraform Variables

```bash
cd Infrastructure
cp terraform.tfvars.example terraform.tfvars
```

Edit `terraform.tfvars` with your specific values:
```hcl
# Environment settings
environment = "dev"
aws_region  = "us-east-1"

# Application settings
app_name = "forecastflow"
domain_name = "your-domain.com"  # Optional

# Database settings
db_username = "forecastflow_admin"
db_password = "your-secure-password"

# OpenWeatherMap API Key
openweather_api_key = "your-openweathermap-api-key"
```

#### 2. Initialize and Plan Terraform

```bash
cd Infrastructure
terraform init
terraform plan
```

#### 3. Deploy Infrastructure

```bash
terraform apply
```

Type `yes` when prompted to confirm the deployment.

#### 4. Deploy Application Code

The deployment script will handle this:
```bash
./deploy.sh dev us-east-1
```

### Manual Deployment Steps (Alternative)

If you prefer manual deployment:

#### Deploy Backend to AWS Lambda
```bash
cd src/ForecastFlow.Lambda
dotnet lambda package
aws lambda update-function-code --function-name ForecastFlow-WeatherProcessor --zip-file fileb://bin/Release/net8.0/ForecastFlow.Lambda.zip
```

#### Deploy Frontend to S3/CloudFront
```bash
cd src/ForecastFlow.Client
npm run build
aws s3 sync dist/ s3://your-bucket-name --delete
aws cloudfront create-invalidation --distribution-id YOUR_DISTRIBUTION_ID --paths "/*"
```

## 🔧 Troubleshooting

### Common Issues and Solutions

#### Database Connection Issues
```bash
# Test database connection
dotnet ef database update --dry-run

# Reset database
dotnet ef database drop
dotnet ef database update
```

#### Node.js/npm Issues
```bash
# Clear npm cache
npm cache clean --force

# Delete node_modules and reinstall
rm -rf node_modules package-lock.json
npm install
```

#### SSL Certificate Issues (Local Development)
```bash
# Trust the .NET development certificate
dotnet dev-certs https --trust
```

#### Port Already in Use
```bash
# Find process using port 5000/7089
netstat -ano | findstr :5000
netstat -ano | findstr :7089

# Kill the process (Windows)
taskkill /PID <process_id> /F

# Kill the process (Linux/Mac)
kill -9 <process_id>
```

### Getting Help

1. **Check the logs**
   - Backend: Console output when running `dotnet run`
   - Frontend: Browser Developer Tools Console
   - AWS: CloudWatch Logs

2. **Common Configuration Files to Check**
   - `src/ForecastFlow.Api/appsettings.json`
   - `src/ForecastFlow.Api/appsettings.Development.json`
   - `src/ForecastFlow.Client/src/environments/environment.ts`

3. **Verify Environment Setup**
   ```bash
   # Check .NET version
   dotnet --version
   
   # Check Node.js version  
   node --version
   npm --version
   
   # Check database connection
   sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT @@VERSION"
   ```

## 📝 Next Steps

After successful setup:

1. **Explore the API** using Swagger UI at `https://localhost:7089/swagger`
2. **Create a user account** through the Angular frontend
3. **Add some tasks** and test the weather integration
4. **Review the code structure** to understand the architecture
5. **Run the tests** to ensure everything is working correctly

For production deployment, make sure to:
- Use proper SSL certificates
- Configure production database with appropriate security
- Set up monitoring and logging
- Implement proper backup strategies
- Review and harden security settings

Happy coding! 🚀