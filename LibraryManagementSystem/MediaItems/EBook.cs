namespace LibraryManagementSystem.MediaItems;

/// <summary>
/// E-book media type.
/// </summary>
public class EBook : Media
{
    public string Author { get; }

    public double FileSize { get; }

    public override int LoanPeriodDays => 21;

    public EBook(string title, int publicationYear, string author, double fileSize)
        : base(title, publicationYear)
    {
        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Author cannot be empty.");

        if (fileSize <= 0)
            throw new ArgumentException("File size must be greater than 0 MB.");

        Author = author;
        FileSize = fileSize;
    }

    public override void DisplayInfo()
    {
        Console.WriteLine($"[{MediaId}] E-Book: '{Title}' by {Author} - {FileSize:0.0} MB");
    }
}
