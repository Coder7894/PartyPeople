using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Website.DTOs;
using Website.Models;
using Website.Persistence;
using Website.Repositories;

namespace Website.Controllers
{
    public class EmployeeEventController : Controller
    {
        private readonly DbContext _dbContext;
        private readonly ILogger<EventController> _logger;

        public EmployeeEventController(DbContext dbContext, ILogger<EventController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmployeeEvent employeeEvent, CancellationToken cancellationToken)
        {
            var employeeExists = await _dbContext.Employees.ExistsAsync(employeeEvent.EmployeeId, cancellationToken);
            if (!employeeExists)
                return NotFound();

            var eventExists = await _dbContext.Employees.ExistsAsync(employeeEvent.EmployeeId, cancellationToken);
            if (!eventExists)
                return NotFound();

            var eventIsAtCapacity = await _dbContext.Events.IsAtCapacityAsync(employeeEvent.EventId, cancellationToken);
            if (eventIsAtCapacity)
            {
                return BadRequest("Event has reached maximum capacity.");
            }

            var createdId = await _dbContext.EmployeeEvents.CreateAsync(employeeEvent, cancellationToken);
            var createdData = await _dbContext.EmployeeEvents.GetOneAsync(createdId);

            eventIsAtCapacity = await _dbContext.Events.IsAtCapacityAsync(employeeEvent.EventId, cancellationToken);

            return Ok( new {Success = true, data = createdData, eventIsAtCapacity = eventIsAtCapacity });
        }


        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.EmployeeEvents.ExistsAsync(id, cancellationToken);
            if (!exists)
                return NotFound();

            await _dbContext.EmployeeEvents.DeleteAsync(id, cancellationToken);
            return Ok( new { Success = true });
        }

        public async Task<ActionResult> GetValidEmployeeOptionsForEvent(int eventId, string? searchTerm, CancellationToken cancellationToken)
        {
            var data = await _dbContext.EmployeeEvents.GetAllEmployeesNotInEventAsync(eventId,searchTerm, cancellationToken);

            return Json(data.Select(x => new SelectOption { Id = x.Id, Text = $"{x.FirstName} {x.LastName}" }));
        }

        public async Task<ActionResult> GetMostSocialEmployees(int take, CancellationToken cancellationToken)
        {
            var data = await _dbContext.EmployeeEvents.GetMostSocialEmployeesAsync(take, cancellationToken);

            return Json(data.Select(x => new ListWidgetDto { Id = x.Id, Text = $"{x.FirstName} {x.LastName}: {x.EventCount} Events!"}));
        }

        public async Task<ActionResult> GetEventsWithNoEmployees(int take, CancellationToken cancellationToken)
        {
            var data = await _dbContext.EmployeeEvents.GetEventsWithNoEmployeesAsync(take, cancellationToken);

            return Json(data.Select(x => new ListWidgetDto { Id = x.Id, Text = $"{x.Description}" }));
        }
    }
}
