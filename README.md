# MasarApp

A desktop application (WPF) for managing graduation projects, built using Clean Architecture with .NET 8, Entity Framework Core, and Oracle.

## Requirements

- Operating System: Windows
- .NET SDK 8.0 or later
- Oracle Database (e.g., Oracle XE/Free)
- (Optional) Visual Studio 2022 for development and running via the UI

## Running the Project

1. Open the terminal inside the project folder:

```bash
cd MasarApp
```

2. Restore packages:

```bash
dotnet restore Masar.sln
```

3. Build the project:

```bash
dotnet build Masar.sln -c Debug
```

4. Run the application:

```bash
dotnet run --project Masar.UI/Masar.UI.csproj
```

Upon startup, the application automatically:

- Applies migrations
- Creates/updates Views, Procedures, Functions, and Triggers
- Creates a default administrator account (if not already present)

## Changing the Database Name and Password (Connection Data)

There are two important places that must be updated:

### 1) Application Runtime Settings

File: `Masar.UI/appsettings.json`

Modify the `MasarDb` value inside `ConnectionStrings`:

```json
{
  "ConnectionStrings": {
    "MasarDb": "User Id=MASAR;Password=masar;Data Source=192.168.56.101:1521/freepdb1"
  }
}
```

### 2) EF Core Design-Time Tools Settings

File: `Masar.Infrastructure/DesignTimeDbContextFactory.cs`

Modify the connection string inside `UseOracle(...)`:

```csharp
optionsBuilder.UseOracle(@"User Id=masar;Password=masar;Data Source=192.168.56.101:1521/FREEPDB1");
```

## Connection String Components Explanation

- `User Id`: Database Username/Schema
- `Password`: Database User Password
- `Data Source`: Oracle address format `HOST:PORT/SERVICE_NAME`

Example:

```text
User Id=NEW_SCHEMA;Password=NEW_PASSWORD;Data Source=127.0.0.1:1521/FREEPDB1
```

## Important Notes

- After changing connection details, restart the application.
- When using `dotnet ef` commands, the connection data in `DesignTimeDbContextFactory` must be correct.
