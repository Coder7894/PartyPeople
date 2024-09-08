namespace Website.Models;

public class HomeViewModel
{
    public required IEnumerable<Event> UpcomingEvents { get; init; }
    public required IEnumerable<Event> UpcomingEventsWithoutAttendees { get; init; }
    public required IReadOnlyCollection<(Employee, long)> EmployeesWithEventCount { get; init; }
}
