# Kakeibo --- Personal Finance Manager

![.NET](https://img.shields.io/badge/.NET_9-512BD4?logo=dotnet&logoColor=white)
![ASP.NET
Core](https://img.shields.io/badge/ASP.NET_Core_9-5C2D91?logo=dotnet)
![Microsoft SQL
Server](https://img.shields.io/badge/SQL_Server-CC2927?logo=microsoftsqlserver&logoColor=white)
![CSharp](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)

Kakeibo is a full--stack personal finance management system inspired by
the traditional Japanese budgeting method.\
It provides expense tracking, category organization, authentication, and
full CRUD capabilities for all finance‑related entities.

------------------------------------------------------------------------

## 🚀 Features

### 🔐 Authentication & Security

-   User registration & login
-   Secure password hashing
-   Password reset with email token
-   JWT authentication (when implemented)

### 💰 Financial Management

-   CRUD for:
    -   Expenses\
    -   Categories\
    -   Payment Methods\
    -   Accounts (if added later)
-   Filtering, sorting and searching
-   Model validation

### 🧱 Backend

-   ASP.NET Core 9 MVC + REST endpoints
-   Entity Framework Core 9
-   SQL Server database (LocalDB or container)
-   Repository & Service layer patterns
-   Full async/await support
-   Clean separation of concerns

### 🧩 Frontend (Client)

-   ASP.NET MVC Razor Frontend
-   Clean UI for entering, editing, and viewing expenses
-   Client‑side validation
-   REST communication with the API

------------------------------------------------------------------------

## 📁 Project Structure

    /Kakeibo
    │
    ├── Kakeibo.Api/            # REST API (ASP.NET 9)
    │   ├── Controllers/
    │   ├── Services/
    │   ├── Repositories/
    │   ├── Entities/
    │   ├── DTO/
    │   └── Kakeibo.Api.csproj
    │
    ├── Kakeibo.Web/            # MVC frontend client
    │   ├── Controllers/
    │   ├── Views/
    │   ├── ViewModels/
    │   └── Kakeibo.Web.csproj
    │
    ├── Kakeibo.Tests/          # Unit tests (xUnit)
    │
    └── README.md               # You are here

------------------------------------------------------------------------

## 🛠️ Tech Stack

  Layer      Technology
  ---------- ------------------------
  Frontend   ASP.NET MVC 9 (Razor)
  API        ASP.NET Core Web API 9
  Database   SQL Server
  ORMs       EF Core 9
  Auth       ASP.NET Identity
  Language   C# (.NET 9)

------------------------------------------------------------------------

## 📦 Setup Instructions

### 1. Clone the repository

``` bash
git clone https://github.com/YOUR_USER/kakeibo.git
cd kakeibo
```

### 2. Configure AppSettings

Both *API* and *Web* projects require valid:

-   SQL Server connection strings\
-   Identity settings\
-   Client URLs\
-   Email provider credentials (for password reset)

### 3. Apply Migrations

Inside the API project:

``` bash
dotnet ef database update --project Kakeibo.Api
```

### 4. Run the Projects

Run both API and Web:

``` bash
dotnet run --project Kakeibo.Api
dotnet run --project Kakeibo.Web
```

The Web app will communicate with the API for all data operations.

------------------------------------------------------------------------

## 🔧 Development Notes

-   Password reset uses an encoded identity token passed on the URL:

        /resetpassword/{userId}?token={encodedToken}

-   When calling `GeneratePasswordResetTokenAsync`, the token **never
    persists** after leaving the endpoint.\
    It must be **stored or transmitted immediately** (usually via
    email).

-   The API uses DTOs for all external communication --- no direct
    entity exposure.

------------------------------------------------------------------------

## 📘 Future Improvements

-   Dashboard with charts
-   Monthly report generation
-   Export to CSV/Excel
-   Budgeting goals
-   Mobile‑friendly UI
-   Import from credit card/OFX files

------------------------------------------------------------------------

## 🤝 Contributing

Pull requests are welcome. For major changes, open an issue first to
discuss what you would like to modify.

------------------------------------------------------------------------

## 📄 License

MIT License --- feel free to use, modify, and distribute.

------------------------------------------------------------------------

## 🙏 Acknowledgements

Inspired by the Japanese household accounting method 家計簿 (*Kakeibo*).
