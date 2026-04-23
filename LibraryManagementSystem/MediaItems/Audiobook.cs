namespace LibraryManagementSystem.MediaItems;

/// <summary>
/// Audiobook media type.
/// </summary>
public class Audiobook : Media
{
    public string Author { get; }

    public TimeSpan Duration { get; }

    public override int LoanPeriodDays => 7;

    public Audiobook(string title, int publicationYear, string author, TimeSpan duration)
        : base(title, publicationYear)
    {
        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Author cannot be empty.");

        if (duration <= TimeSpan.Zero)
            throw new ArgumentException("Duration must be greater than 0.");

        Author = author;
        Duration = duration;
    }

    public override void DisplayInfo()
    {
        Console.WriteLine($"[{MediaId}] Audiobook: '{Title}' by {Author} - {Duration}");
    }
}
