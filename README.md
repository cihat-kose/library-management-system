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

## How to Run
```bash
dotnet restore
dotnet build
dotnet run --project LibraryManagementSystem
```

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
