using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Website.Models;
using Website.Persistence;

namespace Website.Controllers
{
    public class EventController : Controller
    {
        private readonly DbContext _dbContext;
        private readonly IValidator<Event> _validator;
        private readonly ILogger<EventController> _logger;

        public EventController(DbContext dbContext, IValidator<Event> validator, ILogger<EventController> logger)
        {
            _dbContext = dbContext;
            _validator = validator;
            _logger = logger;
        }

        // GET: Event
        public async Task<ActionResult> Index([FromQuery] bool showHistoricEvents = false, CancellationToken cancellationToken = default)
        {
            var events = await _dbContext.Events.GetAllAsync(showHistoricEvents, cancellationToken);
            var eventEmployeeCounts = await _dbContext.EmployeeEvent.GetEmployeesAtEventCountAsync(events.Select(@event => @event.Id), cancellationToken);
            return View(new EventListViewModel { IsShowingHistoricEvents = showHistoricEvents, Events = events, EventEmployeeCounts = eventEmployeeCounts });
        }

        // GET: Event/Details/5
        public async Task<ActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.Events.ExistsAsync(id, cancellationToken);
            if (!exists)
                return NotFound();

            var @event = await _dbContext.Events.GetByIdAsync(id, cancellationToken);
            var employees = await _dbContext.EmployeeEvent.GetEmployeesAtEventAsync(id, cancellationToken);
            return View(new EventDetailsViewModel { Event = @event, Employees = employees });
        }

        // GET: Event/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Event @event, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(@event, cancellationToken);

            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(ModelState);
                return View(@event);
            }

            var createdEvent = await _dbContext.Events.CreateAsync(@event, cancellationToken);

            return RedirectToAction(nameof(Details), new { id = createdEvent.Id });
        }

        // GET: Event/Edit/5
        public async Task<ActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.Events.ExistsAsync(id, cancellationToken);
            if (!exists)
                return NotFound();

            var @event = await _dbContext.Events.GetByIdAsync(id, cancellationToken);
            if (@event?.StartDateTime < DateTime.Now)
                return RedirectToAction(nameof(Details), new { id });

            var employees = await _dbContext.Employees.GetAllAsync(cancellationToken);
            var employeeEvent = await _dbContext.EmployeeEvent.GetEmployeesAtEventAsync(id, cancellationToken);

            var employeeAttendance = employees.ToDictionary(
                employee => employee.Id,
                employee => employeeEvent.Any(e => e.Id== employee.Id)
            );

            return View(new EventEditViewModel { Event = @event, Employees = employees, EmployeeAttendance = employeeAttendance });
        }

        // POST: Event/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, EventEditViewModel eventEdits, CancellationToken cancellationToken)
        {
            // Note: Could use dto?
            var validationResult = await _validator.ValidateAsync(eventEdits.Event, cancellationToken);

            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(ModelState, "Event");
                eventEdits.Employees = await _dbContext.Employees.GetAllAsync(cancellationToken);
                return View(eventEdits);
            }

            // Update the event
            var updatedEvent = await _dbContext.Events.UpdateAsync(eventEdits.Event, cancellationToken);

            // Prevent more possible attendees than the maximum capacity
            if (eventEdits.EmployeeAttendance.Count(e => e.Value) > updatedEvent.MaximumCapacity)
            {
                ModelState.AddModelError("Event.MaximumCapacity", "The number of attendees exceeds the maximum capacity provided.");
                eventEdits.Employees = await _dbContext.Employees.GetAllAsync(cancellationToken);
                return View(eventEdits);
            }

            // Delete associations for employees that are no longer attending
            var currentEmployeeAttending = await _dbContext.EmployeeEvent.GetEmployeesAtEventAsync(id, cancellationToken);
            var employeeIdsToRemove = currentEmployeeAttending
                .Where(e => !eventEdits.EmployeeAttendance[e.Id] || !eventEdits.EmployeeAttendance.ContainsKey(e.Id))
                .Select(e => e.Id)
                .ToArray();

            await _dbContext.EmployeeEvent.DeleteManyAsync(employeeIdsToRemove, cancellationToken);

            // Add associations for employees that are now attending
            if (eventEdits.EmployeeAttendance != null)
            {
                var employeeEventsToAdd = eventEdits.EmployeeAttendance
                    .Where(ep => ep.Value && !currentEmployeeAttending.Any(e => e.Id == ep.Key))
                    .Select(ep => new EmployeeEvent { EmployeeId = ep.Key, EventId = updatedEvent.Id })
                    .ToArray();

                await _dbContext.EmployeeEvent.CreateManyAsync(employeeEventsToAdd, cancellationToken);
            }

            return RedirectToAction(nameof(Details), new { id = updatedEvent.Id });
        }

        // GET: Event/Delete/5
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.Events.ExistsAsync(id, cancellationToken);
            if (!exists)
                return NotFound();

            await _dbContext.Events.DeleteAsync(id, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
    }
}
