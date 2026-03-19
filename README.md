# Getting Started

This guide assumes you're using the repo from the project root. VS Code is recommended for the best experience (Run and Debug, C# tooling).

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/en-us/download/dotnet) **10.0** (or version in the project)
- [Docker](https://docs.docker.com/get-docker/) (for database and services)

## Setup Steps

1. **Install .NET SDK** (if needed)

    [Official Download Page](https://dotnet.microsoft.com/en-us/download/dotnet) — use .NET 10.0.

2. **Start Docker services** (database, networks, etc.)

    ```bash
    docker compose -f compose.yml up -d --build
    ```

3. **Add your admin user to the seeder** (do this *before* running the DB script in step 5)

    In [AdminSeeder.cs](Adapters/Database/Seeders/AdminSeeder.cs), add a new entry to the `Admins` list, e.g.:

    ```csharp
    new("your.email@company.com", "YourPassword123.", "YourFirst", "YourLast"),
    ```

4. **Configure environment variables**

    Copy [.env.example](.env.example) to `.env` and fill in the values. Ask in the Teams channel for shared/secrets values.

5. **Initialize the database**

    Choose one:

    ```bash
    # Local .NET SDK execution (host environment)
    ./scripts/cli nukeAndPave
    ```

    ```bash
    # Docker execution (recommended when API is running in compose)
    ./scripts/cli-docker nukeAndPave
    ```

    ```powershell
    # Docker execution on Windows
    ./scripts/cli-docker.ps1 nukeAndPave
    ```

6. **Run the API** (Swagger UI)

    - **VS Code:** Open **Run and Debug**, then start **Debug API** (or press F5).
    - **Terminal:** `dotnet run` (or run the API project from your IDE).

    Then open the Swagger URL shown in the console (typically `https://localhost:5xxx/swagger` or similar).

7. **Verify login**

    In Swagger, try logging in with the admin user you added in step 3. If it works, you're ready to work on tickets. If not, ask for help in the Teams channel.
