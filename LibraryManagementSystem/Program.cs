using LibraryManagementSystem.Core;
using LibraryManagementSystem.MediaItems;
using LibraryManagementSystem.Users;

namespace LibraryManagementSystem;

internal static class Program
{
    private static void Main()
    {
        var library = new Library();
        SeedDemoData(library);

        while (true)
        {
            ShowMenu();
            var choice = Console.ReadLine()?.Trim();

            try
            {
                switch (choice)
                {
                    case "1":
                        library.ShowAvailableMedia();
                        break;
                    case "2":
                        BorrowFlow(library);
                        break;
                    case "3":
                        ReturnFlow(library);
                        break;
                    case "4":
                        ShowUserLoansFlow(library);
                        break;
                    case "5":
                        AddMediaFlow(library);
                        break;
                    case "6":
                        RegisterUserFlow(library);
                        break;
                    case "7":
                        library.ShowLoanHistory();
                        break;
                    case "8":
                        library.ShowOverdueLoans();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid menu selection.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static void SeedDemoData(Library library)
    {
        var staffUser = new Employee("Admin", "admin@library.com");
        var memberUser = new Member("Demo Member", "member@example.com");

        library.RegisterUser(staffUser);
        library.RegisterUser(memberUser);

        library.AddMedia(new Book("1984", 1949, "George Orwell", 328), staffUser);
        library.AddMedia(new Audiobook("Sapiens", 2011, "Yuval Noah Harari", new TimeSpan(15, 17, 0)), staffUser);
        library.AddMedia(new EBook("Clean Code", 2008, "Robert C. Martin", 2.8), staffUser);
        library.AddMedia(new Journal("National Geographic", DateTime.Today.Year, 3, "March"), staffUser);
    }

    private static void ShowMenu()
    {
        Console.WriteLine();
        Console.WriteLine("=== Library Management System ===");
        Console.WriteLine("1) Show available media");
        Console.WriteLine("2) Borrow media");
        Console.WriteLine("3) Return media");
        Console.WriteLine("4) Show user loans");
        Console.WriteLine("5) Add media (employee only)");
        Console.WriteLine("6) Register user");
        Console.WriteLine("7) Show loan history");
        Console.WriteLine("8) Show overdue loans");
        Console.WriteLine("0) Exit");
        Console.Write("Select: ");
    }

    private static void BorrowFlow(Library library)
    {
        var userId = ReadRequiredText("User ID (e.g., U001): ");
        var mediaId = ReadRequiredText("Media ID (e.g., M001): ");
        library.BorrowMedia(mediaId, userId);
    }

    private static void ReturnFlow(Library library)
    {
        var userId = ReadRequiredText("User ID (e.g., U001): ");
        var mediaId = ReadRequiredText("Media ID (e.g., M001): ");
        library.ReturnMedia(mediaId, userId);
    }

    private static void ShowUserLoansFlow(Library library)
    {
        var userId = ReadRequiredText("User ID (e.g., U001): ");
        library.ShowUserLoans(userId);
    }

    private static void AddMediaFlow(Library library)
    {
        var employeeUserId = ReadRequiredText("Employee User ID: ");
        var user = library.UserRegistry.FirstOrDefault(u => u.UserId.Equals(employeeUserId, StringComparison.OrdinalIgnoreCase));

        if (user is null)
        {
            Console.WriteLine("No user found with this ID.");
            return;
        }

        if (user is not Employee)
        {
            Console.WriteLine("Only employees can add media.");
            return;
        }

        Console.WriteLine("Select media type: 1=Book, 2=Audiobook, 3=E-Book, 4=Journal");
        Console.Write("Type: ");
        var type = Console.ReadLine()?.Trim();

        var title = ReadRequiredText("Title: ");
        var publicationYear = ReadInt("Publication year: ");

        Media media = type switch
        {
            "1" => CreateBook(title, publicationYear),
            "2" => CreateAudiobook(title, publicationYear),
            "3" => CreateEBook(title, publicationYear),
            "4" => CreateJournal(title, publicationYear),
            _ => throw new ArgumentException("Invalid media type.")
        };

        library.AddMedia(media, user);
        Console.WriteLine("Success: media added.");
    }

    private static void RegisterUserFlow(Library library)
    {
        Console.WriteLine("Select user type: 1=Member, 2=Employee");
        Console.Write("Type: ");
        var type = Console.ReadLine()?.Trim();

        var name = ReadRequiredText("Name: ");
        var email = ReadRequiredText("Email: ");

        User newUser = type switch
        {
            "1" => new Member(name, email),
            "2" => new Employee(name, email),
            _ => throw new ArgumentException("Invalid user type.")
        };

        library.RegisterUser(newUser);
        Console.WriteLine($"Success: user registered with ID {newUser.UserId}.");
    }

    private static Book CreateBook(string title, int publicationYear)
    {
        var author = ReadRequiredText("Author: ");
        var pages = ReadInt("Page count: ");
        return new Book(title, publicationYear, author, pages);
    }

    private static Audiobook CreateAudiobook(string title, int publicationYear)
    {
        var author = ReadRequiredText("Author: ");
        var hours = ReadInt("Duration hours: ");
        var minutes = ReadInt("Duration minutes: ");
        return new Audiobook(title, publicationYear, author, new TimeSpan(hours, minutes, 0));
    }

    private static EBook CreateEBook(string title, int publicationYear)
    {
        var author = ReadRequiredText("Author: ");
        var fileSize = ReadDouble("File size (MB): ");
        return new EBook(title, publicationYear, author, fileSize);
    }

    private static Journal CreateJournal(string title, int publicationYear)
    {
        var issueNumber = ReadInt("Issue number: ");
        var month = ReadRequiredText("Month: ");
        return new Journal(title, publicationYear, issueNumber, month);
    }

    private static string ReadRequiredText(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(input))
                return input;

            Console.WriteLine("Value cannot be empty.");
        }
    }

    private static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();

            if (int.TryParse(input, out var value))
                return value;

            Console.WriteLine("Please enter a valid whole number.");
        }
    }

    private static double ReadDouble(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();

            if (double.TryParse(input, out var value))
                return value;

            Console.WriteLine("Please enter a valid number.");
        }
    }
}
