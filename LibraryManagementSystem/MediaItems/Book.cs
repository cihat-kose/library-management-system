namespace LibraryManagementSystem.MediaItems;

/// <summary>
/// Book media type.
/// </summary>
public class Book : Media
{
    public string Author { get; }

    public int PageCount { get; }

    public override int LoanPeriodDays => 14;

    public Book(string title, int publicationYear, string author, int pageCount)
        : base(title, publicationYear)
    {
        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Author cannot be empty.");

        if (pageCount <= 0)
            throw new ArgumentException("Page count must be greater than 0.");

        Author = author;
        PageCount = pageCount;
    }

    public override void DisplayInfo()
    {
        Console.WriteLine($"[{MediaId}] Book: '{Title}' by {Author} ({PublicationYear}) - {PageCount} pages");
    }
}
