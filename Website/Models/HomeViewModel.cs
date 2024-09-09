namespace Website.Models;

/// <summary>
/// The home view model.
/// </summary>
public class HomeViewModel
{
    /// <summary>
    /// The upcoming events (events at some point in the future).
    /// </summary>
    public required IEnumerable<Event> UpcomingEvents { get; init; }

    /// <summary>
    /// The upcoming events without attendees (same as <see cref="UpcomingEvents"/> but without attendees).
    /// </summary>
    public required IEnumerable<Event> UpcomingEventsWithoutAttendees { get; init; }

    /// <summary>
    /// The employees with the number of events they have historically attended.
    /// </summary>
    public required IReadOnlyCollection<(Employee, long)> EmployeesWithEventCount { get; init; }
}
