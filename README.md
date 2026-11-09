# RaceDay – Event Management System

## Project overview

RaceDay is a web-based event management system designed for the South African road running, walking and cycling community. It provides two main user roles: **Organisers** and **Participants**.

Organisers can create and manage their events, set up event categories, view participant enrolments, update enrolment statuses and capture race results. Participants can browse upcoming events, filter events, enrol in a category, view their enrolments and results, and manage their profile.

This repository contains the three parts of the RaceDay POE:

- **Part 1 – Planning and database design:** ERD, API endpoint plan, SQL script, data dictionary and design decisions.
- **Part 2 – RESTful API:** ASP.NET Core Web API, SQL Server database access, authentication, role protection and automated unit tests.
- **Part 3 – MVC web application:** ASP.NET Core MVC user interface that consumes the Part 2 API, uses Azure Blob Storage through the API for images, and includes Docker configuration.

## Main features

### Organiser

- Register and log in using an Organiser account.
- View a dashboard showing event and enrolment information.
- Create, edit and delete events.
- Add and manage categories for an event.
- View enrolments for events they created.
- Confirm or leave enrolments pending.
- Capture participant finish times and finishing positions.
- Upload event banner images.

### Participant

- Register and log in using a Participant account.
- Browse upcoming events.
- Filter events by search text, event type and date range.
- View event details and available categories.
- Enrol in an event by selecting a category.
- View personal event enrolments and their statuses.
- View published results, including finish time and finishing position.
- View and update profile details.
- Upload a profile picture.

### Authentication and role-based access

The API uses session-based authentication. After a successful login, the MVC application stores the user's ID, role and name in its own session. It also retains and forwards the API session cookie when making API requests.

The MVC application uses role protection to restrict Organiser and Participant pages. The API also checks the user's session and role before allowing protected operations. Logging out clears the MVC session and sends a logout request to the API.

## Technologies and tools

- C#
- .NET 8 / ASP.NET Core Web API
- ASP.NET Core MVC
- Entity Framework Core 8
- Microsoft SQL Server
- SQL Server Management Studio (SSMS)
- Azure Blob Storage
- Bootstrap 5.3
- HTML and CSS
- Docker and Docker Compose
- Git and GitHub
- GitHub Actions
- xUnit unit testing
- Swagger / OpenAPI

## System architecture

The MVC application does not connect directly to the SQL Server database or Azure Blob Storage. It sends requests to the REST API. The API handles business operations, communicates with SQL Server through Entity Framework Core, and uploads images to Azure Blob Storage.

```text
Participant / Organiser
          |
          v
   RaceDay MVC Web App
       (Part 3)
          |
          | HTTP API requests
          v
     RaceDay REST API
        (Part 2)
        /       \
       v         v
 SQL Server   Azure Blob Storage
 Database     (event banners and
               profile pictures)
```

The MVC application forwards the API session cookie for authenticated requests. Image uploads follow the route MVC → API → Azure Blob Storage. The API stores the returned image URL in the relevant database record.

## Repository structure

```text
RaceDay-POE/
│
├── .github/
│   └── workflows/
│       ├── build-and-test.yml
│       └── validate-docs.yml
│
├── docs/
│   ├── RaceDay_ERD.png
│   ├── RaceDay_API_Endpoint_Plan.pdf
│   ├── RaceDay_Database_Script.sql
│   ├── RaceDay_Data_Dictionary.md
│   ├── RaceDay_Design_Decisions.md
│   ├── RaceDay_Verification_Queries.sql
│   └── CI/CD evidence
│
├── Part2-API/
│   ├── Controllers/
│   ├── Data/
│   ├── Dtos/
│   ├── Filters/
│   ├── Helpers/
│   ├── Migrations/
│   ├── Models/
│   ├── Services/
│   ├── RaceDay.Api.Tests/
│   ├── Program.cs
│   └── RaceDay.Api.csproj
│
└── Part3-Web/
    ├── Controllers/
    ├── Filters/
    ├── Models/
    ├── Services/
    ├── Views/
    ├── Dockerfile
    ├── docker-compose.yml
    ├── .dockerignore
    ├── Program.cs
    └── RaceDay.Web.csproj
```

## Requirements

Install the following before running the project locally:

- Visual Studio 2022 with the ASP.NET and web development workload, or the .NET 8 SDK.
- SQL Server (for example, SQL Server Express).
- SQL Server Management Studio.
- Docker Desktop, if you want to run the MVC application in a container.
- An Azure Storage Account with Blob Storage access, for image uploads.

## Database configuration

The API uses SQL Server and Entity Framework Core migrations.

1. Open `Part2-API/appsettings.json`.
2. Update `ConnectionStrings:DefaultConnection` so that the `Server` value matches the SQL Server instance installed on your computer.
3. Keep your own local connection details out of public source control if they contain sensitive information.
4. Apply the EF Core migrations to create or update the database.

From the repository root, the command-line migration command is:

```powershell
dotnet tool install --global dotnet-ef
dotnet ef database update --project Part2-API/RaceDay.Api.csproj
```

If the `dotnet-ef` tool is already installed, you do not need to install it again.

The Part 1 SQL script is available at `docs/RaceDay_Database_Script.sql`. The API project also contains EF Core migrations, including the Part 3 additions for enrolment status and image URLs. Use the database setup approach specified for your current project state; do not create the same schema twice.

## Azure Blob Storage configuration

The API uses Azure Blob Storage for event banners and participant profile pictures. The MVC application does not connect to Azure directly.

The API reads its Azure connection string from the configuration key:

```text
AzureStorage:ConnectionString
```

For local development, configure this value using .NET User Secrets rather than committing a real connection string to GitHub.

From the repository root, run:

```powershell
dotnet user-secrets init --project Part2-API/RaceDay.Api.csproj
dotnet user-secrets set "AzureStorage:ConnectionString" "YOUR_AZURE_STORAGE_CONNECTION_STRING" --project Part2-API/RaceDay.Api.csproj
```

Replace the placeholder with the connection string from your own Azure Storage Account. Do not publish the connection string, account key or other credentials.

The API uploads images to these Blob containers:

- `event-banners`
- `profile-pictures`

The upload endpoint accepts JPG, JPEG, PNG, GIF and WEBP images up to 5 MB. The API saves the resulting image URL in the database.

## Running the application locally

Run the API and MVC application as separate projects.

### 1. Run the API

From the repository root:

```powershell
dotnet run --project Part2-API/RaceDay.Api.csproj --launch-profile http
```

The HTTP launch profile is configured for:

```text
http://localhost:5218
```

In Development mode, the API Swagger page is available at:

```text
http://localhost:5218/swagger
```

Make sure SQL Server is running, the database has been created or migrated, and the API configuration is correct.

### 2. Run the MVC application

Open a second terminal window at the repository root and run:

```powershell
dotnet run --project Part3-Web/RaceDay.Web.csproj
```

The MVC HTTP launch profile is configured for:

```text
http://localhost:5245
```

Open that address in a browser. The MVC application calls the API at `http://localhost:5218` by default.

### 3. Create test accounts

Use the Register page to create accounts with the appropriate roles:

- `Organiser`
- `Participant`

Log in separately with each role to view the role-specific navigation and pages.

## Running the MVC application with Docker

The Docker configuration in `Part3-Web` containerises the MVC application. The API and SQL Server are not started by this Compose file; the API is expected to be running separately and accessible from the Docker container.

1. Start Docker Desktop and wait until the Docker Engine is running.
2. Start the API on `http://localhost:5218`.
3. Open PowerShell in the `Part3-Web` folder.
4. Run:

```powershell
docker compose up --build
```

5. Open the MVC application at:

```text
http://localhost:8081
```

The Compose configuration maps host port `8081` to container port `8080` and sets the API address to `http://host.docker.internal:5218`.

To stop the container, press `Ctrl + C` in the PowerShell window. To remove the stopped Compose resources, run:

```powershell
docker compose down
```

## Automated testing and CI/CD

The repository uses GitHub Actions workflows stored in `.github/workflows`.

The `build-and-test.yml` workflow:

1. Checks out the repository.
2. Sets up .NET 8.
3. Restores the API test project.
4. Builds the API.
5. Builds and runs the API unit tests.
6. Restores and builds the MVC project.
7. Builds the MVC Docker image.

The API unit tests are located in `Part2-API/RaceDay.Api.Tests`. Run them locally from the repository root with:

```powershell
dotnet test Part2-API/RaceDay.Api.Tests/RaceDay.Api.Tests.csproj
```

The repository also includes CI evidence screenshots in the project folders. The live status of the workflow can be checked under the **Actions** tab of the GitHub repository.

## Part 1 documentation

The `docs` folder contains the planning and database design deliverables:

- Entity Relationship Diagram (ERD)
- REST API endpoint plan
- SQL Server database script
- Data dictionary
- Database design decisions
- Verification queries

## Project repository

GitHub: https://github.com/ST10474835/RaceDay-POE

## Video demonstration

The Part 3 video demonstration link should be added here after the video has been uploaded to YouTube as an **Unlisted** video.

**YouTube video:** (https://youtu.be/qj95jsBI_kc)
