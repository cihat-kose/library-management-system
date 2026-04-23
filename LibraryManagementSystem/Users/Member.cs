namespace LibraryManagementSystem.Users;

/// <summary>
/// Member role (Member) with standard borrowing limits.
/// </summary>
public class Member : User
{
    private const int MaxBorrowedItems = 5;

    public Member(string name, string email) : base(name, email)
    {
    }

    public override bool CanBorrow()
    {
        return BorrowedItems.Count < MaxBorrowedItems;
    }
}
