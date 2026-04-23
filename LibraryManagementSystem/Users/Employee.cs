namespace LibraryManagementSystem.Users;

/// <summary>
/// Staff role (Employee) with elevated borrowing capacity.
/// </summary>
public class Employee : User
{
    private const int MaxBorrowedItems = 10;

    public Employee(string name, string email) : base(name, email)
    {
    }

    public override bool CanBorrow()
    {
        return BorrowedItems.Count < MaxBorrowedItems;
    }
}
