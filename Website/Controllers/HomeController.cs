using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Website.Models;
using Website.Persistence;

namespace Website.Controllers;

public class HomeController : Controller
{
    private readonly DbContext _dbContext;
    private readonly ILogger<HomeController> _logger;

    public HomeController(DbContext dbContext, ILogger<HomeController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var events = await _dbContext.Events.GetAllAsync(includeHistoricEvents: false, cancellationToken);
        var upcomingEvents = events.Where(@event => @event.StartDateTime >= DateTime.Now && @event.StartDateTime <= DateTime.Now.AddDays(7));
        var upcomingEventAttendeeCount = await _dbContext.EmployeeEvent.GetEmployeesAtEventCountAsync(upcomingEvents.Select(@event => @event.Id), cancellationToken);
        var upcomingEventsWithoutAttendees = upcomingEvents.Where(@event => !upcomingEventAttendeeCount.ContainsKey(@event.Id));
        var employeesWithEventCount = await _dbContext.Employees.GetTopEmployeesByEventsAttendedAsync(5, cancellationToken);

        return View(new HomeViewModel {
            UpcomingEvents = upcomingEvents,
            UpcomingEventsWithoutAttendees = upcomingEventsWithoutAttendees,
            EmployeesWithEventCount = employeesWithEventCount
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
