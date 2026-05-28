# ForecastFlow Technical Documentation

## 📖 Overview
ForecastFlow is a smart daily planner that integrates weather forecasts with task management. It helps users optimize their schedules by providing weather-aware suggestions for their planned activities.

## 🏗️ Architecture

### System Components
1.  **Frontend (Angular SPA)**: A mobile-first interface built with Angular 20.1 and Angular Material.
2.  **Backend API (.NET 8.0)**: Core service handling authentication, task management, and coordination between components.
3.  **Lambda Processor (C#)**: A background processing component (simulated locally) that analyzes weather data and generates suggestions.
4.  **Database (SQLite/SQL Server)**: Persistence layer. SQLite is used for local development, while SQL Server is supported for production.
5.  **Messaging Queue (Mock SQS)**: An in-memory queue that decouples task creation from weather processing.

### Data Flow (The "Happy Path")
1.  **User Authentication**: User logs in/registers via `AuthController`, receiving a JWT token.
2.  **Task Creation**: User creates a task with a location. The `TasksController` saves the task to the DB and pushes a message to the `IMessageQueueService`.
3.  **Background Processing**: The `MockSqsBackgroundWorker` picks up the message and invokes the `WeatherProcessor` (Lambda logic).
4.  **Weather Integration**: `WeatherProcessor` calls the `OpenWeatherService` to get a 5-day forecast.
5.  **Suggestion Generation**: Based on the weather (temperature, conditions), the processor generates a list of actionable suggestions.
6.  **Async Update**: The processor calls a `PATCH` endpoint on the API to update the task's `SuggestionsJson` field.
7.  **UI Update**: The Angular dashboard polls or refreshes to display the newly generated suggestions.

## 🛠️ Technical Details

### Backend Implementation
- **Repository Pattern**: Used to abstract data access (`IAppTaskRepository`, `IAppUserRepository`).
- **Dependency Injection**: Extensively used for services like `IWeatherService`, `IMessageQueueService`, and `HttpClient`.
- **JWT Authentication**: Secure endpoints using standard Bearer token validation.
- **SQLite Support**: Configured to automatically use SQLite if the connection string contains `.db`, facilitating zero-setup development.

### Mock SQS & Background Worker
To enable full local testing without AWS dependencies, we implemented:
- `IMessageQueueService`: Interface for sending/receiving messages.
- `InMemoryMessageQueueService`: A thread-safe `ConcurrentQueue` implementation.
- `MockSqsBackgroundWorker`: A `BackgroundService` that runs continuously, processing messages and simulating Lambda triggers.

### Weather Integration
- **OpenWeatherMap API**: Integrated via `OpenWeatherService`.
- **Fallback Logic**: If the API is unavailable or the key is invalid, the system falls back to realistic mock data to ensure a smooth development experience.

### Frontend Implementation
- **Reactive Forms**: Used for robust validation in Login and Task creation.
- **HTTP Interceptor**: `JwtInterceptor` automatically adds the Bearer token to all outgoing requests.
- **Auth Guard**: `AuthGuard` protects private routes from unauthenticated access.
- **Polling**: The dashboard implements a simple polling mechanism to reflect background weather processing updates.

## 🚀 Getting Started for Developers

### Prerequisites
- .NET 8.0 SDK
- Node.js 20+
- A valid OpenWeatherMap API key (set in `appsettings.json`)

### Backend Setup
1.  Navigate to `src/ForecastFlow.Api`.
2.  Run `dotnet restore`.
3.  Run `dotnet ef database update` to initialize the SQLite database.
4.  Run `dotnet run`.

### Frontend Setup
1.  Navigate to `src/ForecastFlow.Client`.
2.  Run `npm install --legacy-peer-deps`.
3.  Run `npm start`.

## 📈 Future Enhancements
- Real AWS SQS & Lambda deployment via Terraform.
- More advanced AI suggestion logic (e.g., using OpenAI).
- Push notifications for weather alerts.
- Map integration for location picking.
