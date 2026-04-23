using LibraryManagementSystem.Loans;
using LibraryManagementSystem.MediaItems;
using LibraryManagementSystem.Users;

namespace LibraryManagementSystem.Core;

/// <summary>
/// Central service that manages media, users, and loan operations.
/// </summary>
public class Library
{
    private readonly List<Media> _mediaRegistry = new();
    private readonly List<User> _userRegistry = new();
    private readonly List<Loan> _loanHistory = new();

    /// <summary>
    /// In-memory store for all media items.
    /// </summary>
    public IReadOnlyList<Media> MediaRegistry => _mediaRegistry;

    /// <summary>
    /// In-memory store for all users.
    /// </summary>
    public IReadOnlyList<User> UserRegistry => _userRegistry;

    /// <summary>
    /// In-memory store for all loan transactions.
    /// </summary>
    public IReadOnlyList<Loan> LoanHistory => _loanHistory;

    /// <summary>
    /// Adds a media item to the catalog.
    /// Only <see cref="Employee"/> users are allowed to add media.
    /// </summary>
    public void AddMedia(Media media, User user)
    {
        ArgumentNullException.ThrowIfNull(media);
        ArgumentNullException.ThrowIfNull(user);

        if (user is not Employee)
            throw new InvalidOperationException("Only Employee users can add media.");

        _mediaRegistry.Add(media);
    }

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    public void RegisterUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var emailAlreadyExists = _userRegistry.Any(existingUser =>
            existingUser.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase));

        if (emailAlreadyExists)
            throw new InvalidOperationException("A user with this email address is already registered.");

        _userRegistry.Add(user);
    }

    /// <summary>
    /// Borrows a media item for a user if all loan rules are satisfied.
    /// Also creates and stores a loan history record.
    /// </summary>
    public void BorrowMedia(string mediaId, string userId)
    {
        var user = FindUser(userId);
        var media = FindMedia(mediaId);

        if (media.IsLoaned)
            throw new InvalidOperationException("This media item is already loaned out.");

        if (!user.CanBorrow())
            throw new InvalidOperationException("User has reached the borrowing limit.");

        media.MarkAsLoaned();
        user.BorrowedItems.Add(media);

        var loan = new Loan(media, user, DateTime.Today);
        _loanHistory.Add(loan);

        Console.WriteLine($"LOG: Borrow | User={user.UserId} | Media={media.MediaId} | Date={loan.LoanDate:yyyy-MM-dd}");
        Console.WriteLine($"Success: '{media.Title}' borrowed by {user.Name}. Due: {loan.ExpectedReturnDate:yyyy-MM-dd}");
    }

    /// <summary>
    /// Returns a media item and updates the corresponding active loan record.
    /// </summary>
    public void ReturnMedia(string mediaId, string userId)
    {
        var user = FindUser(userId);
        var media = FindMedia(mediaId);

        if (!media.IsLoaned)
            throw new InvalidOperationException("This media item is not currently loaned.");

        if (!user.BorrowedItems.Contains(media))
            throw new InvalidOperationException("This media item is not loaned by the specified user.");

        media.MarkAsAvailable();
        user.BorrowedItems.Remove(media);

        var activeLoan = _loanHistory.LastOrDefault(l => l.Media == media && l.User == user && l.ReturnedDate is null);
        activeLoan?.MarkReturned(DateTime.Today);

        Console.WriteLine($"LOG: Return | User={user.UserId} | Media={media.MediaId} | Date={DateTime.Today:yyyy-MM-dd}");
        Console.WriteLine($"Success: '{media.Title}' returned by {user.Name}.");
    }

    /// <summary>
    /// Displays all currently available media items.
    /// </summary>
    public void ShowAvailableMedia()
    {
        var available = _mediaRegistry.Where(m => !m.IsLoaned).ToList();

        if (available.Count == 0)
        {
            Console.WriteLine("No available media found.");
            return;
        }

        Console.WriteLine("=== Available Media ===");
        foreach (var media in available)
            media.DisplayInfo();
    }

    /// <summary>
    /// Displays all active loans for a specific user.
    /// </summary>
    public void ShowUserLoans(string userId)
    {
        var user = FindUser(userId);

        if (user.BorrowedItems.Count == 0)
        {
            Console.WriteLine("No active loans for this user.");
            return;
        }

        Console.WriteLine("=== Active User Loans ===");
        foreach (var media in user.BorrowedItems)
        {
            var activeLoan = _loanHistory.LastOrDefault(l => l.Media == media && l.User == user && l.ReturnedDate is null);
            if (activeLoan is null)
            {
                Console.WriteLine($"[{media.MediaId}] '{media.Title}'");
                continue;
            }

            var daysLeft = (activeLoan.ExpectedReturnDate.Date - DateTime.Today).Days;
            Console.WriteLine($"[{media.MediaId}] '{media.Title}' | Due: {activeLoan.ExpectedReturnDate:yyyy-MM-dd} | Days left: {daysLeft}");
        }
    }

    /// <summary>
    /// Displays complete loan history.
    /// </summary>
    public void ShowLoanHistory()
    {
        if (_loanHistory.Count == 0)
        {
            Console.WriteLine("No loan records yet.");
            return;
        }

        Console.WriteLine("=== Loan History ===");
        foreach (var loan in _loanHistory)
        {
            var status = loan.ReturnedDate is null ? "ACTIVE" : $"RETURNED {loan.ReturnedDate:yyyy-MM-dd}";
            Console.WriteLine($"[{loan.LoanId}] {loan.User.UserId} -> {loan.Media.MediaId} | {loan.LoanDate:yyyy-MM-dd} | Due: {loan.ExpectedReturnDate:yyyy-MM-dd} | {status}");
        }
    }

    /// <summary>
    /// Displays loans that are overdue and not yet returned.
    /// </summary>
    public void ShowOverdueLoans()
    {
        var overdue = _loanHistory.Where(l => l.IsOverdue()).ToList();

        if (overdue.Count == 0)
        {
            Console.WriteLine("No overdue loans.");
            return;
        }

        Console.WriteLine("=== Overdue Loans ===");
        foreach (var loan in overdue)
        {
            var daysOverdue = (DateTime.Today - loan.ExpectedReturnDate.Date).Days;
            Console.WriteLine($"[{loan.LoanId}] {loan.User.UserId} -> {loan.Media.MediaId} | Due: {loan.ExpectedReturnDate:yyyy-MM-dd} | Overdue: {daysOverdue} days");
        }
    }

    private User FindUser(string userId)
    {
        return _userRegistry.FirstOrDefault(u => u.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase))
               ?? throw new ArgumentException("User not found.");
    }

    private Media FindMedia(string mediaId)
    {
        return _mediaRegistry.FirstOrDefault(m => m.MediaId.Equals(mediaId, StringComparison.OrdinalIgnoreCase))
               ?? throw new ArgumentException("Media not found.");
    }
}
