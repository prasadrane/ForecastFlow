# ForecastFlow 🌤️📋

*A smart daily planner that integrates weather forecasts with task management, providing location-based activity suggestions.*

[![.NET](https://img.shields.io/badge/.NET-9.0-blue)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-20.1-red)](https://angular.io/)
[![AWS](https://img.shields.io/badge/AWS-Lambda%20%7C%20SQS-orange)](https://aws.amazon.com/)
[![Terraform](https://img.shields.io/badge/Infrastructure-Terraform-purple)](https://terraform.io/)

## 🎯 Overview

ForecastFlow revolutionizes daily planning by seamlessly combining weather forecasts with task management. Users can create location-based tasks and receive intelligent, weather-aware activity suggestions to optimize their daily schedules.

### Key Features

- **📍 Location-Based Tasks**: Create tasks tied to specific locations (e.g., "Hike in Central Park")
- **🌦️ Weather Integration**: Real-time weather data from OpenWeatherMap API
- **🤖 Smart Suggestions**: AI-generated activity recommendations based on weather conditions
- **📱 Responsive Design**: Beautiful, mobile-first interface built with Angular Material
- **🔔 Weather Alerts**: Automatic notifications when weather affects planned activities
- **🔐 Secure Authentication**: JWT-based user authentication and authorization

<!-- Architecture Diagram Placeholder -->
## 🏗️ Architecture

*[Architecture diagram to be added]*

The application follows a modern microservices architecture with clear separation of concerns:

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Angular SPA   │────│  .NET Web API   │────│   SQL Server    │
│   (Frontend)    │    │   (Backend)     │    │   (Database)    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │
         │              ┌─────────────────┐
         │              │   AWS Lambda    │
         │              │  (Processing)   │
         │              └─────────────────┘
         │                       │
┌─────────────────┐    ┌─────────────────┐
│  Weather API    │    │    AWS SQS      │
│ (OpenWeather)   │    │  (Messaging)    │
└─────────────────┘    └─────────────────┘
```

## 🛠️ Tech Stack

### Backend (.NET Web API)
- **Framework**: .NET 9.0 Web API
- **Database**: Entity Framework Core with SQL Server
- **Authentication**: JWT Bearer tokens
- **Cloud Services**: AWS SDK for SQS and Secrets Manager
- **API Documentation**: Swagger/OpenAPI

### Frontend (Angular)
- **Framework**: Angular 20.1 with TypeScript
- **UI Components**: Angular Material + Flex Layout
- **State Management**: RxJS for reactive programming
- **Responsive Design**: Mobile-first approach
- **Build Tools**: Angular CLI with modern build pipeline

### AWS Infrastructure
- **Compute**: AWS Lambda (C#) for serverless processing
- **Messaging**: AWS SQS for asynchronous task processing
- **Secrets**: AWS Secrets Manager for secure configuration
- **Infrastructure**: Terraform for Infrastructure as Code

### External APIs
- **Weather Data**: OpenWeatherMap API for real-time weather information

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) *(Note: Project targets .NET 9.0 but can be built with .NET 8.0+ by updating target framework)*
- [Node.js 20+](https://nodejs.org/) and npm
- [SQL Server](https://www.microsoft.com/en-us/sql-server) (LocalDB or full instance)
- [AWS CLI](https://aws.amazon.com/cli/) (for deployment)
- [Terraform](https://terraform.io/) (for infrastructure)

### 🔧 Backend Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/prasadrane/ForecastFlow.git
   cd ForecastFlow
   ```

2. **Restore .NET packages**
   ```bash
   cd src
   dotnet restore
   ```
   
   *Note: If using .NET 8.0 SDK, you may need to update the target framework in the `.csproj` files from `net9.0` to `net8.0`*

3. **Configure database connection**
   Update `appsettings.json` in `ForecastFlow.Api`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ForecastFlowDb;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

4. **Run database migrations**
   ```bash
   cd ForecastFlow.Api
   dotnet ef database update
   ```

5. **Set up OpenWeatherMap API key**
   ```bash
   dotnet user-secrets set "OpenWeatherMap:ApiKey" "your-api-key-here"
   ```

6. **Run the API**
   ```bash
   dotnet run --project ForecastFlow.Api
   ```
   The API will be available at `https://localhost:7089`

### 🎨 Frontend Setup

1. **Navigate to the client directory**
   ```bash
   cd src/ForecastFlow.Client
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Start the development server**
   ```bash
   npm start
   ```
   The application will be available at `http://localhost:4200`

### 📱 Application Demo

*[GIF demo of the working app to be added]*

## 📋 API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/refresh` - Refresh JWT token

### Tasks Management
- `GET /api/tasks` - Get user's tasks
- `GET /api/tasks/{id}` - Get specific task
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task

### Weather & Suggestions
- `GET /api/weather/current/{lat}/{lon}` - Get current weather
- `GET /api/weather/forecast/{lat}/{lon}` - Get weather forecast
- `GET /api/suggestions/{taskId}` - Get weather-based suggestions

## 🧪 Development

### Running Tests

**Backend Tests**
```bash
cd src
dotnet test
```

**Frontend Tests**
```bash
cd src/ForecastFlow.Client
npm test
```

### Code Quality

**Backend Linting**
```bash
dotnet format
```

**Frontend Linting**
```bash
cd src/ForecastFlow.Client
ng lint
```

### Building for Production

**Backend**
```bash
cd src
dotnet build -c Release
```

**Frontend**
```bash
cd src/ForecastFlow.Client
npm run build
```

## 🚀 Deployment

### Terraform Infrastructure

*[Terraform deployment instructions to be added]*

The infrastructure includes:
- AWS Lambda functions for weather processing
- SQS queues for asynchronous messaging
- RDS instance for production database
- Application Load Balancer for high availability
- CloudWatch for monitoring and logging

### Manual Deployment Steps

1. **Deploy Backend to AWS Lambda**
   ```bash
   # Instructions coming soon with Terraform setup
   ```

2. **Deploy Frontend to S3/CloudFront**
   ```bash
   # Instructions coming soon with Terraform setup
   ```

## 🎓 Lessons Learned

*[Lessons Learned section to be added]*

This section will cover:
- Challenges faced during development
- Performance optimization insights
- Best practices discovered
- Architecture decisions and trade-offs

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [OpenWeatherMap](https://openweathermap.org/) for weather data API
- [Angular Material](https://material.angular.io/) for UI components
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) for data access
- AWS for cloud infrastructure services

---

**Built with ❤️ for developers who want to plan their day around the weather!**