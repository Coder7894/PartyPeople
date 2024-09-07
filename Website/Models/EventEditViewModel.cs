namespace Website.Models;

public class EventEditViewModel
{
    public required Event Event { get; init; }
    public required IReadOnlyCollection<Employee> Employees { get; init; }
    public required Dictionary<int, bool> EmployeeAttendance { get; init; }
}
