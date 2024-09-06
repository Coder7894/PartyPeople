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
            return View(@event);
        }

        // POST: Event/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, Event @event, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(@event, cancellationToken);

            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(ModelState);
                return View(@event);
            }

            var updatedEvent = await _dbContext.Events.UpdateAsync(@event, cancellationToken);

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
