# Sub Zero POS

Desktop Point-of-Sale system for **Sub Zero** (سوب زيرو) — tracks orders, income, expenses, and daily reports for a single-location shop.

## Tech Stack

- **UI:** WPF (.NET 8), MVVM via CommunityToolkit.Mvvm
- **Data Access:** Entity Framework Core 8, SQL Server
- **Architecture:** 3-layer (Core / Data / WPF) — see below
- **Auth:** Local login with BCrypt-hashed passwords, role-based (Manager / Cashier)

## Project Structure

```
SubZeroPOS.sln
│
├── SubZeroPOS.Core/        Entities, DTOs, service interfaces (no external dependencies)
│   ├── Entities/           User, Role, Category, Item, Order, OrderItem, Expense, etc.
│   ├── DTOs/                Data transfer objects (AuthResultDto, CreateOrderDto, etc.)
│   └── Interfaces/          Service contracts (IAuthService, IOrderService, etc.)
│
├── SubZeroPOS.Data/         EF Core DbContext, entity configurations, service implementations
│   ├── Configurations/      Fluent API config per entity (matches SQL schema exactly)
│   ├── Services/             Concrete service implementations (AuthService, etc.)
│   ├── SubZeroDbContext.cs
│   └── SubZeroDbContextFactory.cs   (design-time factory for EF migrations)
│
└── SubZeroPOS.WPF/           Presentation layer
    ├── Views/                 XAML views
    ├── ViewModels/             MVVM ViewModels
    ├── Converters/             XAML value converters
    ├── Resources/              Theme.xaml (Sub Zero brand colors)
    ├── Session/                CurrentSession (logged-in user state)
    └── App.xaml.cs             DI container setup
```

**Dependency direction:** `WPF → Data → Core` and `WPF → Core`. `Core` has no dependencies on anything else.

## Naming Conventions

- Table/column names are in **English** for clean tooling and C# code.
- Arabic display data uses an `Ar` suffix **only when a genuine bilingual pair exists** (e.g. `Items.NameAr` + `Items.NameEn`). A field with only one language has no suffix (e.g. `Users.FullName`).
- Internal/logic-facing values (roles, order types, statuses) have a plain `Code` column for C# comparisons, plus a `NameAr` column for what the UI displays. Example: `Roles.RoleCode` ("Manager") vs `Roles.RoleNameAr` ("مدير").
- Always `NVARCHAR` (never `VARCHAR`) for any column storing Arabic text.

## Getting Started

### Prerequisites
- Visual Studio 2022 (with .NET desktop development + WPF workload)
- SQL Server / SQL Server Express (local instance)
- .NET 8 SDK

### Setup
1. Clone the repo and open `SubZeroPOS.sln`
2. Update the connection string in **two places** (kept in sync manually for now):
   - `SubZeroPOS.Data/SubZeroDbContextFactory.cs` (used by EF Core CLI for migrations)
   - `SubZeroPOS.WPF/App.xaml.cs` (used by the running app)
3. From the `SubZeroPOS.Data` folder, run:
   ```
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
4. Seed real BCrypt password hashes for the default `admin` / `cashier1` users (see `Users` table).
5. Run the `SubZeroPOS.WPF` project.

## Roadmap

- [x] Database schema (Core entities + Data configurations)
- [x] Login screen + role-based session
- [ ] Order Entry (POS) screen
- [ ] Order Details / Receipt view
- [ ] Expenses (add/list)
- [ ] Main Dashboard
- [ ] Shift Open/Close
- [ ] Reports (income vs expenses)
- [ ] User Management / Menu Management / Settings

## Roles

| Role | Access |
|---|---|
| **Manager (مدير)** | Full access: POS, expenses, reports, user/menu management |
| **Cashier (كاشير)** | POS only |
