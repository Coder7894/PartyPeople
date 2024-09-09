namespace Website.Models;

/// <summary>
/// The event edit view model.
/// </summary>
public class EventEditViewModel
{
    /// <summary>
    /// The event.
    /// </summary>
    public required Event Event { get; init; }
    
    /// <summary>
    /// All the available employees to attend the event.
    /// </summary>
    public IReadOnlyCollection<Employee>? Employees { get; set; }
    
    /// <summary>
    /// The employees that are attending the event as a key value pair.
    /// ID: Employee ID, Value: Is employee attending the event?
    /// </summary>
    public required Dictionary<int, bool> EmployeeAttendance { get; init; }
}
