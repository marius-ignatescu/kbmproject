# KBMProject

Dockerized gRPC microservices application with HTTP gateway and SQL Server backend. This system allows creating, querying, and managing users and organizations, including features like audit trails, soft deletes, and pagination.

---

## Features

- gRPC-based services for "User" and "Organization" management
- HTTP API Gateway (via gRPC client) for REST consumers
- SQL Server with Entity Framework Core (code-first)
- Soft delete support via "DeletedAt" field
- Audit log system (for inserts/updates)
- Pagination, filtering, sorting
- Centralized error handling (gRPC + HTTP)
- Clean separation of concerns (Services, Utils, Validators)
- Dockerized multi-service environment with "docker-compose"

---

## Technologies Used

- .NET 9
- gRPC
- ASP.NET Core
- Entity Framework Core
- SQL Server 2017
- Docker & Docker Compose
- Fluent validation & error handling
- LINQ.Dynamic.Core (for dynamic sorting)

---

## Project Structure
- KBMProject / docker-compose # Docker orchestration files
- KBMProject / KBMGrpcService # gRPC microservice
- KBMProject / KBMHttpService # HTTP API gateway for gRPC
- KBMProject / KBMContracts # Contains Data Transfer Objects
- KBMProject / KBMProtos # Contains the proto files for the gRPC microservice
---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

---

### Create the .env file
To avoid storing sensitive credentials it is used a .env file. Create the .env file in "docker-compose" folder with the following content:
```bash
# docker-compose/.env
SA_PASSWORD=YourPasswordHere
DB_USER=kbm_user
DB_USER_PASSWORD=YourPasswordHere
DB_NAME=kbmdb
```

### Run with Docker Compose

```bash
docker-compose up --build
```

### Database Setup
This project uses SQL Server running in a Docker container. On startup, an initialization script generated inside entrypoint.sh is automatically executed to:
- Create the kbmdb database
- Create a SQL login kbm_user that will be used by the microservice
- Grant it access to the database

The script the SQL commands to initialize the database and create the kbm_user login with proper permissions.
The Docker container mounts this script and runs it at startup.

#### Run EF Core Migrations
To apply the Entity Framework Core migrations run this command in Package Manager Console:
```bash
$env:DB_HOST="127.0.0.1"
$env:DB_NAME="kbmdb"
$env:DB_USER="kbm_user"
$env:DB_USER_PASSWORD="YourPasswordHere"
Update-Database
```

## Database Persistence
The SQL Server database data is persisted using Docker volumes. This ensures that data is not lost when the container is stopped or restarted.

## API Overview

### gRPC Service (KBMGrpcService)

- **UserService**
- CreateUser
- GetUserById
- QueryUsers
- UpdateUser
- DeleteUser
- AssociateUserToOrganization
- DisassociateUserFromOrganization
- QueryUsersForOrganization

- **OrganizationService**
- CreateOrganization
- GetOrganizationById
- QueryOrganizations
- UpdateOrganization
- DeleteOrganization

Protos are defined in KBMGrpcService/Protos.

### HTTP API (KBMHttpService)
The HTTP API acts as a gateway for the underlying gRPC services. It exposes RESTful endpoints and includes built-in Swagger UI for easy exploration and testing.
Swagger is available by default at: http://localhost:5000

## Tests

This project includes both **unit tests** and **integration tests** using **xUnit** and **EF Core InMemory**.

### Unit Tests
The unit tests are located in `KBMGrpcService.Tests` project.

### Integration Tests

These tests exercise full gRPC service methods using real in-memory data context.
- `UserServiceTests`
- `UserServiceNegativeTests`
- `OrganizationServiceTests`
- `OrganizationServiceNegativeTests`

# State management
State is persisted via SQL Server. Entities include:
Users
Organizations
AuditLogs

The application handles state transitions (create/update/delete), soft delete logic, and audit logging.

# Notes
Errors are handled with gRPC exceptions and middleware in HTTP API.
All database entities include: CreatedAt, UpdatedAt, DeletedAt for auditing.
