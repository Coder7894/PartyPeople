namespace Website.Models;

/// <summary>
/// The employee list view model.
/// </summary>
public class EmployeeListViewModel
{
    /// <summary>
    /// The employees to show in the list.
    /// </summary>
    public required IReadOnlyCollection<Employee> Employees { get; init; }
}
