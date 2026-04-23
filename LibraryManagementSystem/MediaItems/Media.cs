namespace LibraryManagementSystem.MediaItems;

/// <summary>
/// Abstract base class for all library media.
/// Demonstrates abstraction + encapsulation.
/// </summary>
public abstract class Media
{
    private static int _idCounter = 1;

    private readonly string _mediaId;
    private string _title = string.Empty;
    private int _publicationYear;

    protected Media(string title, int publicationYear)
    {
        _mediaId = $"M{_idCounter:D3}";
        _idCounter++;

        Title = title;
        PublicationYear = publicationYear;
        IsLoaned = false;
    }

    /// <summary>
    /// Unique, auto-generated media identifier.
    /// </summary>
    public string MediaId => _mediaId;

    public string Title
    {
        get => _title;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Title cannot be empty.");

            _title = value;
        }
    }

    public int PublicationYear
    {
        get => _publicationYear;
        set
        {
            const int minYear = 1800;
            var maxYear = DateTime.Today.Year;

            if (value < minYear || value > maxYear)
                throw new ArgumentException($"Publication year must be between {minYear} and {maxYear}.");

            _publicationYear = value;
        }
    }

    public bool IsLoaned { get; protected set; }

    public abstract int LoanPeriodDays { get; }

    public abstract void DisplayInfo();

    public void MarkAsLoaned() => IsLoaned = true;

    public void MarkAsAvailable() => IsLoaned = false;
}
