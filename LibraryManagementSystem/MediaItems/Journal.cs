using System.Globalization;

namespace LibraryManagementSystem.MediaItems;

/// <summary>
/// Journal issue media type.
/// </summary>
public class Journal : Media
{
    public int IssueNumber { get; }

    public string Month { get; }

    public override int LoanPeriodDays => 3;

    public Journal(string title, int publicationYear, int issueNumber, string month)
        : base(title, publicationYear)
    {
        if (issueNumber <= 0)
            throw new ArgumentException("Issue number must be greater than 0.");

        if (string.IsNullOrWhiteSpace(month))
            throw new ArgumentException("Month cannot be empty.");

        if (!DateTime.TryParseExact(month.Trim(), "MMMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            throw new ArgumentException("Month must be a full English month name (for example: March).");

        IssueNumber = issueNumber;
        Month = month.Trim();
    }

    public override void DisplayInfo()
    {
        Console.WriteLine($"[{MediaId}] Journal: '{Title}' - Issue {IssueNumber}, {Month} {PublicationYear}");
    }
}
