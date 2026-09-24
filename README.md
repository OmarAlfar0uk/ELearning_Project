# 🎓 ELearning_Project

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Entity Framework Core](https://img.shields.io/badge/EF_Core-8.0-388E3C?style=for-the-badge&logo=nuget&logoColor=white)](https://docs.microsoft.com/en-us/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/en-us/sql-server)

**ELearning_Project** is a full-featured E-Learning platform API providing comprehensive course management, gamification, and student evaluation. It supports multi-track learning paths, secure file uploads, interactive assignments, and robust notification systems to deliver a modern educational experience.

## 🏗️ Architecture & Flow

```mermaid
graph TD
    Client((Client App)) --> |REST API| API[ELearning API]
    API --> |CQRS Pipeline| MediatR[MediatR]
    MediatR --> |Request Validation| Fluent[FluentValidation]
    MediatR --> |Repository Pattern| Repos[Repositories]
    Repos --> |Data Access| EF[EF Core 8]
    EF --> DB[(SQL Server)]
    
    API --> |Authentication| Auth[Identity & JWT]
    API --> |File Handling| Storage[Local/Cloud Storage]
    API --> |Email| MailKit[MailKit]
```

## ✨ Features

| Feature | Description |
|---------|-------------|
| **Authentication & Auth** | JWT-based secure login, registration, and user profile management. |
| **Learning Tracks & Batches** | Organize students into specific educational tracks and time-based batches. |
| **Exams & Assignments** | Create, manage, and evaluate student exams and assignment submissions. |
| **Gamification (Coins)** | Reward students with digital coins based on performance and engagement. |
| **Student Evaluation** | Detailed evaluation metrics tracking student progress across materials and lectures. |
| **File Management** | Secure file upload and management for educational materials and submissions. |
| **Notifications** | Automated system for delivering important updates and alerts to users. |
| **Articles & Dashboard** | Publish educational articles and provide a comprehensive statistical dashboard. |

## 🛠️ Tech Stack

| Category | Technology |
|----------|------------|
| **Framework** | .NET 8.0, ASP.NET Core Web API |
| **Language** | C# 12 |
| **Architecture** | Repository Pattern, CQRS |
| **Data Access** | Entity Framework Core 8, SQL Server |
| **Identity & Security**| ASP.NET Core Identity, JWT Bearer Authentication |
| **Libraries** | MediatR 10, FluentValidation 12, MailKit |

## 📂 Project Structure

```text
ELearning_Project/
├── Controllers/         # API Endpoints for Exams, Batches, Submissions, etc.
├── Features/            # CQRS commands and queries
├── Repositories/        # Data access abstraction layer
├── Models/              # Domain entities
├── Extensions/          # Utility and setup extensions
└── Program.cs           # Application bootstrap and configuration
```

## 🚀 Getting Started

To get the project up and running locally, execute the following commands:

```bash
git clone https://github.com/OmarAlfar0uk/ELearning_Project.git
cd ELearning_Project
dotnet restore
dotnet run
```

---
**Author**  
GitHub: [OmarAlfar0uk](https://github.com/OmarAlfar0uk) | LinkedIn: [omar-alfarouk-252471251](https://www.linkedin.com/in/omar-alfarouk-252471251/) | Email: [omaralfarouk646@gmail.com](mailto:omaralfarouk646@gmail.com)
