namespace Website.Models;

/// <summary>
/// The event details view model.
/// </summary>
public class EventDetailsViewModel
{
    /// <summary>
    /// The event.
    /// </summary>
    public required Event Event { get; init; }

    /// <summary>
    /// The employees that are attending the event.
    /// </summary>
    public required IReadOnlyCollection<Employee> Employees { get; init; }
}
