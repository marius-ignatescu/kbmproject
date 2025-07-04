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
KBMProject / docker-compose # Docker orchestration files
KBMProject / KBMGrpcService # gRPC microservice
KBMProject / KBMHttpService # HTTP API gateway for gRPC

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

---

### Run with Docker Compose

```bash
docker-compose up --build
```bash

In docker-compose > sql folder there is an initialization script that will run when the container starts and will create the database and the user used by the gRPC service (if doesn't exist).

To apply migrations locally
cd KBMGrpcService
dotnet ef database update
or
update-database (via Package Manager Console)


## Database Persistence
The SQL Server database data is persisted using Docker volumes. This ensures that data is not lost when the container is stopped or restarted.

## API Overview

### gRPC Service (KBMGrpcService)

- UserService
----CreateUser
----GetUserById
----QueryUsers
----UpdateUser
----DeleteUser
----AssociateUserToOrganization
----DisassociateUserFromOrganization
----QueryUsersForOrganization

-OrganizationService
----CreateOrganization
----GetOrganizationById
----QueryOrganizations
----UpdateOrganization
----DeleteOrganization

Protos are defined in KBMGrpcService/Protos.

### HTTP API (KBMHttpService)
The HTTP API acts as a gateway for the underlying gRPC services. It exposes RESTful endpoints and includes built-in Swagger UI for easy exploration and testing.
Swagger is available by default at: http://localhost:5000

# State management
State is persisted via SQL Server. Entities include:
Users
Organizations
AuditLogs

The application handles state transitions (create/update/delete), soft delete logic, and audit logging.

# Environment variables
The connection string is defined in appsettings.json respectivelly appsettings.development.json

# Notes
Errors are handled with gRPC exceptions and middleware in HTTP API.
All database entities include: CreatedAt, UpdatedAt, DeletedAt for auditing.
