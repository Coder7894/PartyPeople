using System.ComponentModel;

namespace Website.Models;

/// <summary>
/// The employee event model.
/// </summary>
public class EmployeeEvent
{
    /// <summary>
    /// The unique identifier for the employee.
    /// </summary>
    public required int EmployeeId { get; init; }

    /// <summary>
    /// The unique identifier for the event.
    /// </summary>
    public required int EventId { get; init; }
}