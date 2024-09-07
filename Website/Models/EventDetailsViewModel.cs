using System.Threading;
using Website.Persistence;
namespace Website.Models;

public class EventDetailsViewModel
{
    public required Event Event { get; init; }
    public required IReadOnlyCollection<Employee> Employees { get; init; }
}
