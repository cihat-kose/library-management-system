using LibraryManagementSystem.Core;
using LibraryManagementSystem.Loans;
using LibraryManagementSystem.MediaItems;
using LibraryManagementSystem.Users;

namespace LibraryManagementSystem.Tests;

// Keep tests sharing the production static ID counters in one xUnit collection.
[Collection("Loan rules")]
public class LoanRulesTests
{
    [Theory]
    [InlineData(false, 5)]
    [InlineData(true, 10)]
    public void Borrow_AtLimit_RejectsWithoutChangingState(bool employee, int limit)
    {
        var (library, user) = CreateLibrary(employee);
        Assert.True(user.CanBorrow());
        for (var i = 0; i < limit; i++)
        {
            Assert.True(user.CanBorrow());
            var media = AddBook(library);
            library.BorrowMedia(media.MediaId, user.UserId);
            Assert.True(media.IsLoaned);
            Assert.Equal(i + 1, user.BorrowedItems.Count);
            var loan = library.LoanHistory[i];
            Assert.Same(media, loan.Media);
            Assert.Same(user, loan.User);
            Assert.Null(loan.ReturnedDate);
        }

        Assert.False(user.CanBorrow());
        var extra = AddBook(library);
        var assertUnchanged = CaptureState(library);
        Assert.Throws<InvalidOperationException>(() => library.BorrowMedia(extra.MediaId, user.UserId));
        assertUnchanged();

        // Returning one item restores exactly one available slot.
        library.ReturnMedia(user.BorrowedItems[0].MediaId, user.UserId);
        Assert.True(user.CanBorrow());
        library.BorrowMedia(extra.MediaId, user.UserId);
        Assert.True(extra.IsLoaned);
        Assert.Contains(extra, user.BorrowedItems);
        Assert.Equal(limit, user.BorrowedItems.Count);
        Assert.False(user.CanBorrow());
        Assert.Equal(limit + 1, library.LoanHistory.Count);
        Assert.NotNull(library.LoanHistory[0].ReturnedDate);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Borrow_AlreadyLoaned_RejectsSameOrDifferentUserWithoutChangingState(bool differentUser)
    {
        var (library, owner) = CreateLibrary();
        var other = new Member("Other", "other@example.com");
        library.RegisterUser(other);
        var media = AddBook(library);
        library.BorrowMedia(media.MediaId, owner.UserId);
        var assertUnchanged = CaptureState(library);

        Assert.Throws<InvalidOperationException>(() =>
            library.BorrowMedia(media.MediaId, (differentUser ? other : owner).UserId));

        assertUnchanged();
    }

    [Fact]
    public void Return_ByWrongUser_RejectsWithoutChangingState()
    {
        var (library, owner) = CreateLibrary();
        var other = new Member("Other", "other@example.com");
        library.RegisterUser(other);
        var media = AddBook(library);
        var otherMedia = AddBook(library);
        library.BorrowMedia(media.MediaId, owner.UserId);
        library.BorrowMedia(otherMedia.MediaId, other.UserId);
        var assertUnchanged = CaptureState(library);

        Assert.Throws<InvalidOperationException>(() => library.ReturnMedia(media.MediaId, other.UserId));

        assertUnchanged();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Return_ThenBorrowAgain_PreservesHistoryAndClosesOnlyActiveLoan(bool differentUser)
    {
        var (library, owner) = CreateLibrary();
        var other = new Member("Other", "other@example.com");
        library.RegisterUser(other);
        var media = AddBook(library);
        library.BorrowMedia(media.MediaId, owner.UserId);
        var first = Assert.Single(library.LoanHistory);
        var originalLoanDate = first.LoanDate;
        var originalDueDate = first.ExpectedReturnDate;

        library.ReturnMedia(media.MediaId, owner.UserId);

        Assert.False(media.IsLoaned);
        Assert.Empty(owner.BorrowedItems);
        Assert.Same(first, Assert.Single(library.LoanHistory));
        Assert.NotNull(first.ReturnedDate);
        var firstReturnDate = first.ReturnedDate;
        var assertUnchanged = CaptureState(library);
        Assert.Throws<InvalidOperationException>(() => library.ReturnMedia(media.MediaId, owner.UserId));
        assertUnchanged();

        var nextUser = differentUser ? other : owner;
        library.BorrowMedia(media.MediaId, nextUser.UserId);
        Assert.True(media.IsLoaned);
        Assert.Same(media, Assert.Single(nextUser.BorrowedItems));
        Assert.Equal(2, library.LoanHistory.Count);
        var second = library.LoanHistory[1];
        Assert.NotSame(first, second);
        Assert.NotEqual(first.LoanId, second.LoanId);
        Assert.Same(nextUser, second.User);
        Assert.Same(media, second.Media);
        Assert.Null(second.ReturnedDate);

        library.ReturnMedia(media.MediaId, nextUser.UserId);
        Assert.False(media.IsLoaned);
        Assert.Empty(owner.BorrowedItems);
        Assert.Empty(other.BorrowedItems);
        Assert.NotNull(second.ReturnedDate);
        Assert.Equal(firstReturnDate, first.ReturnedDate);
        Assert.Equal(originalLoanDate, first.LoanDate);
        Assert.Equal(originalDueDate, first.ExpectedReturnDate);
        Assert.Equal(2, library.LoanHistory.Count);
    }

    [Fact]
    public void Return_NeverBorrowed_RejectsWithoutChangingState()
    {
        var (library, user) = CreateLibrary();
        var media = AddBook(library);
        var assertUnchanged = CaptureState(library);
        Assert.Throws<InvalidOperationException>(() => library.ReturnMedia(media.MediaId, user.UserId));
        assertUnchanged();
    }

    [Theory]
    [InlineData("Book", 14)]
    [InlineData("Audiobook", 7)]
    [InlineData("EBook", 21)]
    [InlineData("Journal", 3)]
    public void DueDate_UsesMediaPeriod_FromExplicitLoanDateAndLibraryLoan(string kind, int days)
    {
        var (library, user) = CreateLibrary();
        Media media = kind switch
        {
            "Book" => new Book("Book", 2020, "Author", 100),
            "Audiobook" => new Audiobook("Audio", 2020, "Author", TimeSpan.FromHours(1)),
            "EBook" => new EBook("EBook", 2020, "Author", 2),
            "Journal" => new Journal("Journal", 2020, 1, "January"),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        Assert.Equal(days, media.LoanPeriodDays);
        // Leap-day and year boundaries; no dependency on today's date.
        foreach (var date in new[] { new DateTime(2024, 2, 27, 13, 45, 0), new DateTime(2024, 12, 30) })
        {
            var loan = new Loan(media, user, date);
            Assert.Equal(date, loan.LoanDate);
            Assert.Equal(date.AddDays(days), loan.ExpectedReturnDate);
            Assert.Null(loan.ReturnedDate);
        }

        library.AddMedia(media, new Employee("Cataloguer", "cataloguer@example.com"));
        library.BorrowMedia(media.MediaId, user.UserId);
        var recorded = Assert.Single(library.LoanHistory);
        Assert.Equal(recorded.LoanDate.AddDays(days), recorded.ExpectedReturnDate);
        Assert.Same(media, recorded.Media);
        Assert.Same(user, recorded.User);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Return_WithoutMatchingActiveLoan_RejectsWithoutChangingState(bool returnedHistoryExists)
    {
        var (library, user) = CreateLibrary();
        var media = AddBook(library);
        if (returnedHistoryExists)
        {
            library.BorrowMedia(media.MediaId, user.UserId);
            library.ReturnMedia(media.MediaId, user.UserId);
        }

        // Public mutation APIs currently allow inconsistent state. This is a
        // defensive test, not a supported way to create a loan.
        media.MarkAsLoaned();
        user.BorrowedItems.Add(media);
        var assertUnchanged = CaptureState(library);

        Assert.Throws<InvalidOperationException>(() => library.ReturnMedia(media.MediaId, user.UserId));

        assertUnchanged();
    }

    [Fact]
    public void Return_WithAnotherUsersActiveLoan_RejectsWithoutChangingState()
    {
        var (library, owner) = CreateLibrary();
        var other = new Member("Other", "other@example.com");
        library.RegisterUser(other);
        var media = AddBook(library);
        library.BorrowMedia(media.MediaId, owner.UserId);
        // Simulate external list corruption without modifying the real loan.
        other.BorrowedItems.Add(media);
        var assertUnchanged = CaptureState(library);

        Assert.Throws<InvalidOperationException>(() => library.ReturnMedia(media.MediaId, other.UserId));

        assertUnchanged();
    }

    private static (Library Library, User User) CreateLibrary(bool employee = false)
    {
        var library = new Library();
        User user = employee
            ? new Employee("Employee", "employee@example.com")
            : new Member("Member", "member@example.com");
        library.RegisterUser(user);
        return (library, user);
    }

    private static Book AddBook(Library library)
    {
        var book = new Book("Test book", 2020, "Author", 100);
        library.AddMedia(book, new Employee("Cataloguer", "cataloguer@example.com"));
        return book;
    }

    // Snapshot values as well as references: comparing only mutable objects could
    // accidentally compare the modified object with itself and hide a regression.
    private static Action CaptureState(Library library)
    {
        var media = library.MediaRegistry.ToArray();
        var users = library.UserRegistry.ToArray();
        var loans = library.LoanHistory.ToArray();
        var availability = media.Select(m => m.IsLoaned).ToArray();
        var borrowed = users.Select(u => u.BorrowedItems.ToArray()).ToArray();
        var dates = loans.Select(l => (l.LoanDate, l.ExpectedReturnDate, l.ReturnedDate)).ToArray();
        return () =>
        {
            Assert.Equal(media, library.MediaRegistry.ToArray());
            Assert.Equal(users, library.UserRegistry.ToArray());
            Assert.Equal(loans, library.LoanHistory.ToArray());
            Assert.Equal(availability, media.Select(m => m.IsLoaned).ToArray());
            for (var i = 0; i < users.Length; i++)
                Assert.Equal(borrowed[i], users[i].BorrowedItems.ToArray());
            Assert.Equal(dates, loans.Select(l => (l.LoanDate, l.ExpectedReturnDate, l.ReturnedDate)).ToArray());
        };
    }
}
