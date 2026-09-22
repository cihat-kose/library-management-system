# 📚 Library Management System

![C#](https://img.shields.io/badge/C%23-.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![App Type](https://img.shields.io/badge/App-Console-blue?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

A console-based library management system built with C#, featuring media catalog management, role-based access control, and loan lifecycle tracking through clean object-oriented design.

## Features
- Manage role-based users (`Member`, `Employee`) with distinct borrowing limits.
- Maintain a media catalog with specialized item types:
  - `Book`
  - `Audiobook`
  - `EBook`
  - `Journal`
- Borrow and return media with business-rule validation.
- Track due dates using media-specific loan periods.
- View active user loans, full loan history, and overdue loans.
- Receive clear runtime feedback for input and validation errors.

## Tech Stack
- **Language:** C#
- **Framework:** .NET 8 (`net8.0`)
- **Application Type:** Console application
- **Data Storage:** In-memory collections (`List<T>`)

## OOP / Design Principles Demonstrated
- **Abstraction:** `User` and `Media` are abstract base classes.
- **Inheritance:** Concrete user and media types extend shared base models.
- **Polymorphism:** Core behaviors are executed through base-type contracts.
- **Encapsulation:** Entities enforce internal validation and state transitions.
- **Single Responsibility:** Domain entities and orchestration logic are separated by purpose.

## Quick start

Install the **.NET 8 SDK** (includes the .NET 8 runtime). A newer SDK can also
build this `net8.0` solution, but the .NET 8 runtime must be available to run it.
Open a terminal in the downloaded repository root, beside
`library-management-system.sln`, then run:

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project LibraryManagementSystem
```

The first restore needs internet access to NuGet for the test packages.
No database, Docker, or external account is required. This is an educational
portfolio project; all data is in memory and resets when the application exits.

Each launch creates these demo users (IDs are entered directly; there is no login):

| ID | Name | Type | Email |
| --- | --- | --- | --- |
| `U001` | Admin | Employee | admin@library.com |
| `U002` | Demo Member | Member | member@example.com |

Try this menu sequence; press Enter after each value:

1. Enter `1` to list the four demo media items, including `M001` (the book *1984*).
2. Enter `2`, then user ID `U002`, then media ID `M001` to borrow it.
3. Enter `4`, then `U002` to see the loan and its due date (14 days after borrowing).
4. Enter `3`, then `U002`, then `M001` to return it.
5. Enter `7` to see the history marked `RETURNED`; enter `1` to see the book available again.
6. Enter `0` to exit.

## Project Structure
```text
LibraryManagementSystem/
├── Core/
│   └── Library.cs
├── Loans/
│   └── Loan.cs
├── MediaItems/
│   ├── Media.cs
│   ├── Book.cs
│   ├── Audiobook.cs
│   ├── EBook.cs
│   └── Journal.cs
├── Users/
│   ├── User.cs
│   ├── Member.cs
│   └── Employee.cs
└── Program.cs
```

## Roadmap
- Add persistent storage (for example SQLite).
- Add unit and integration test coverage.
- Add CI automation for build and test validation.
- Add reporting/export capabilities for inventory and loan analytics.
- Add API and web front-end options.

## Contributing
Contributions are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request.

## Security
For vulnerability reporting guidance, see [SECURITY.md](SECURITY.md).

## License
This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
