using LibraryManagementSystem.Core;
using LibraryManagementSystem.Loans;
using LibraryManagementSystem.MediaItems;
using LibraryManagementSystem.Users;

namespace LibraryManagementSystem.Tests;

[Collection("Loan rules")]
public class LoanDateTests
{
    [Theory]
    [InlineData(14, 0, false)]
    [InlineData(14, 23, false)]
    [InlineData(15, 0, true)]
    public void IsOverdue_UsesCalendarDayBoundary(int day, int hour, bool expected)
    {
        var clock = new TestTimeProvider(new DateTimeOffset(2024, 1, day, hour, 0, 0, TimeSpan.Zero));
        var loan = new Loan(new Book("Book", 2020, "Author", 100),
            new Member("Member", "member@example.com"), new DateTime(2023, 12, 31), clock);

        Assert.Equal(new DateTime(2024, 1, 14), loan.ExpectedReturnDate);
        Assert.Equal(expected, loan.IsOverdue());
    }

    [Fact]
    public void IsOverdue_ReturnedLoan_RemainsFalseAfterDueDate()
    {
        var clock = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero));
        var loan = new Loan(new Book("Book", 2020, "Author", 100),
            new Member("Member", "member@example.com"), new DateTime(2023, 12, 31), clock);
        Assert.True(loan.IsOverdue());

        loan.MarkReturned(new DateTime(2024, 1, 15));

        Assert.False(loan.IsOverdue());
        clock.UtcNow = clock.UtcNow.AddDays(30);
        Assert.False(loan.IsOverdue());
    }

    [Fact]
    public void Library_UsesSameProviderForBorrowReturnAndOverdue_WithLocalCalendarDates()
    {
        // UTC is still December 31; the provider's local date is January 1.
        var zone = TimeZoneInfo.CreateCustomTimeZone("Test UTC+02", TimeSpan.FromHours(2),
            "Test UTC+02", "Test UTC+02");
        var clock = new TestTimeProvider(new DateTimeOffset(2023, 12, 31, 22, 30, 0, TimeSpan.Zero), zone);
        var library = new Library(clock);
        var user = new Member("Member", "member@example.com");
        var book = new Book("Book", 2020, "Author", 100);
        library.RegisterUser(user);
        library.AddMedia(book, new Employee("Employee", "employee@example.com"));

        library.BorrowMedia(book.MediaId, user.UserId);
        var loan = Assert.Single(library.LoanHistory);
        Assert.Equal(new DateTime(2024, 1, 1), loan.LoanDate);
        Assert.Equal(new DateTime(2024, 1, 15), loan.ExpectedReturnDate);

        clock.UtcNow = new DateTimeOffset(2024, 1, 15, 21, 59, 0, TimeSpan.Zero);
        Assert.False(loan.IsOverdue()); // Local January 15, 23:59.
        clock.UtcNow = clock.UtcNow.AddMinutes(1);
        Assert.True(loan.IsOverdue()); // Local January 16, 00:00.

        library.ReturnMedia(book.MediaId, user.UserId);
        Assert.Equal(new DateTime(2024, 1, 16), loan.ReturnedDate);
        Assert.False(loan.IsOverdue());
        Assert.False(book.IsLoaned);
        Assert.Empty(user.BorrowedItems);
    }

    // Only the clock capabilities used by the domain are overridden; no timers or extra package.
    private sealed class TestTimeProvider(DateTimeOffset utcNow, TimeZoneInfo? zone = null) : TimeProvider
    {
        public DateTimeOffset UtcNow { get; set; } = utcNow;
        public override DateTimeOffset GetUtcNow() => UtcNow;
        public override TimeZoneInfo LocalTimeZone => zone ?? TimeZoneInfo.Utc;
    }
}
