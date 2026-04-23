using System.Net.Mail;
using LibraryManagementSystem.MediaItems;

namespace LibraryManagementSystem.Users;

/// <summary>
/// Abstract base class for all users.
/// Demonstrates abstraction and encapsulation.
/// </summary>
public abstract class User
{
    private static int _idCounter = 1;

    private readonly string _userId;
    private string _name = string.Empty;
    private string _email = string.Empty;

    protected User(string name, string email)
    {
        _userId = $"U{_idCounter:D3}";
        _idCounter++;

        Name = name;
        Email = email;
        BorrowedItems = new List<Media>();
    }

    /// <summary>
    /// Unique, auto-generated user identifier.
    /// </summary>
    public string UserId => _userId;

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be empty.");

            _name = value.Trim();
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty.");

            var normalizedEmail = value.Trim();

            try
            {
                var mailAddress = new MailAddress(normalizedEmail);
                if (!mailAddress.Address.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("Email format is invalid.");
            }
            catch (FormatException)
            {
                throw new ArgumentException("Email format is invalid.");
            }

            _email = normalizedEmail;
        }
    }

    public List<Media> BorrowedItems { get; }

    public abstract bool CanBorrow();
}
