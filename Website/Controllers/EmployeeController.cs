using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Website.DTOs;
using Website.Models;
using Website.Persistence;

namespace Website.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly DbContext _dbContext;
        private readonly IValidator<Employee> _validator;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(DbContext dbContext, IValidator<Employee> validator, ILogger<EmployeeController> logger)
        {
            _dbContext = dbContext;
            _validator = validator;
            _logger = logger;
        }

        // GET: Employee
        public async Task<ActionResult> Index( CancellationToken cancellationToken)
        {
            var employees = await _dbContext.Employees.GetAllAsync( cancellationToken);
            return View(new EmployeeListViewModel { Employees = employees });
        }


        // GET: Employee/Details/5
        public async Task<ActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.Employees.ExistsAsync(id, cancellationToken);
            if (!exists)
                return NotFound();

            var employee = await _dbContext.Employees.GetByIdAsync(id, cancellationToken);
            var events = await _dbContext.EmployeeEvents.GetAllAsync(employeeId: id, cancellationToken: cancellationToken);
            employee.Events = events.ToList();
            return View(employee);
        }

        // GET: Employee/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Employee employee, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(employee, cancellationToken);

            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(ModelState);
                return View(employee);
            }

            var createdEmployee = await _dbContext.Employees.CreateAsync(employee, cancellationToken);

            return RedirectToAction(nameof(Details), new { id = createdEmployee.Id });
        }

        // GET: Employee/Edit/5
        public async Task<ActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.Employees.ExistsAsync(id, cancellationToken);
            if (!exists)
                return NotFound();

            var employee = await _dbContext.Employees.GetByIdAsync(id, cancellationToken);
            return View(employee);
        }

        // POST: Employee/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, Employee employee, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(employee, cancellationToken);

            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(ModelState);
                return View(employee);
            }

            var updatedEmployee = await _dbContext.Employees.UpdateAsync(employee, cancellationToken);

            return RedirectToAction(nameof(Details), new { id = updatedEmployee.Id });
        }

        // GET: Employee/Delete/5
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.Employees.ExistsAsync(id, cancellationToken);
            if (!exists)
                return NotFound();

            await _dbContext.Employees.DeleteAsync(id, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
    }
}
