namespace Website.Models;

/// <summary>
/// The event list view model.
/// </summary>
public class EventListViewModel
{
    /// <summary>
    /// If historic events are being shown (events that have already happened).
    /// </summary>
    public required bool IsShowingHistoricEvents { get; init; }

    /// <summary>
    /// The events to show in the list.
    /// </summary>
    public required IReadOnlyCollection<Event> Events { get; init; }

    /// <summary>
    /// The number of employees attending the individual events as a key value pair.
    /// ID: Event ID, Value: Number of employees attending the event.
    /// </summary>
    public required Dictionary<int, int> EventEmployeeCounts { get; init; }
}
