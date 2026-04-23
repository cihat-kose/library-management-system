using LibraryManagementSystem.Users;
using MediaItem = LibraryManagementSystem.MediaItems.Media;

namespace LibraryManagementSystem.Loans;

/// <summary>
/// Represents one loan transaction in the system.
/// </summary>
public class Loan
{
    private static int _idCounter = 1;

    public string LoanId { get; }

    public MediaItem Media { get; }

    public User User { get; }

    public DateTime LoanDate { get; }

    public DateTime ExpectedReturnDate { get; }

    public DateTime? ReturnedDate { get; private set; }

    public Loan(MediaItem media, User user, DateTime loanDate)
    {
        LoanId = $"L{_idCounter:D3}";
        _idCounter++;

        Media = media;
        User = user;
        LoanDate = loanDate;
        ExpectedReturnDate = loanDate.AddDays(media.LoanPeriodDays);
    }

    public void MarkReturned(DateTime returnedDate)
    {
        ReturnedDate = returnedDate;
    }

    public bool IsOverdue()
    {
        return ReturnedDate is null && DateTime.Today > ExpectedReturnDate.Date;
    }
}
