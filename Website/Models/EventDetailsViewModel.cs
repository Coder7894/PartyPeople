using System.Threading;
using Website.Persistence;
namespace Website.Models;

public class EventDetailsViewModel
{
    public Event Event { get; init; }
    public IReadOnlyCollection<Employee> Employees { get; init; }
}
