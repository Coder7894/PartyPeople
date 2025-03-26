using Dapper;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Models;

namespace Website.Test.Repositories
{
    public class EmployeeEventRepositoryTests : TestBase
    {

        #region CreateTableIfNotExistsAsync
        [Fact]
        public async Task CreateTableIfNotExistsAsyncShould_CreateEventsTable()
        {
            await _connection.ExecuteAsync("DROP TABLE IF EXISTS EmployeeEvent;"); //Drop table because table is automatically spun up on Context initialization
            await _context.EmployeeEvents.CreateTableIfNotExistsAsync(CancellationToken.None);

            var tableExists = await _connection.ExecuteScalarAsync<int>(
                "SELECT count(*) FROM sqlite_master WHERE type='table' AND LOWER(name)='employeeevent';"
            ) == 1;

            tableExists.ShouldBeTrue();
        }
        #endregion

        #region GetAllAsync
        [Fact]
        public async Task GetAllAsync_ShouldReturnEntries_WhenEventIdIsSpecifies()
        {
            var event1 = new Event
            {
                Id = 0,
                Description = "Future Event",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now.AddDays(2),
                MaximumCapacity = 100
            };
            var createdEvent1 = await _context.Events.CreateAsync(event1, CancellationToken.None);
            var event2 = new Event
            {
                Id = 0,
                Description = "Future Event",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now.AddDays(2),
                MaximumCapacity = 100
            };
            var createdEvent2 = await _context.Events.CreateAsync(event1, CancellationToken.None);

            var Employee1 = new Employee
            {
                Id = 0,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateOnly(1999, 1, 1),
                FavouriteDrink = "mojito"
            };
            var createdEmployee = await _context.Employees.CreateAsync(Employee1, CancellationToken.None);

            var employeeEvent1 = new EmployeeEvent
            {
                Id = 0,
                EventId = createdEvent1.Id,
                EmployeeId = createdEmployee.Id,
            };

            var employeeEvent2 = new EmployeeEvent
            {
                Id = 0,
                EventId = createdEvent2.Id,
                EmployeeId = createdEmployee.Id,
            };

            var insertQuery = @"
                INSERT INTO EmployeeEvent (EventId, EmployeeId)
                VALUES (@EventId, @EmployeeId);
            ";

            await _connection.ExecuteAsync(insertQuery, employeeEvent1);
            await _connection.ExecuteAsync(insertQuery, employeeEvent2);

            var result = await _context.EmployeeEvents.GetAllAsync(eventId: createdEvent1.Id,null, CancellationToken.None);

            result.ShouldHaveSingleItem();
            result.First().EventId.ShouldBe(createdEvent1.Id);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEntries_WhenEmployeeIdIsSpecified()
        {
            var event1 = new Event
            {
                Id = 0,
                Description = "Future Event",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now.AddDays(2),
                MaximumCapacity = 100
            };
            var createdEvent1 = await _context.Events.CreateAsync(event1, CancellationToken.None);

            var Employee1 = new Employee
            {
                Id = 0,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateOnly(1999, 1, 1),
                FavouriteDrink= "mojito"
            };
            var createdEmployee1 = await _context.Employees.CreateAsync(Employee1, CancellationToken.None);

            var Employee2 = new Employee
            {
                Id = 0,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateOnly(1999, 1, 1),
                FavouriteDrink = "guinness"
            };
            var createdEmployee2 = await _context.Employees.CreateAsync(Employee1, CancellationToken.None);

            var employeeEvent1 = new EmployeeEvent
            {
                Id = 0,
                EventId = createdEvent1.Id,
                EmployeeId = createdEmployee1.Id,
            };

            var employeeEvent2 = new EmployeeEvent
            {
                Id = 0,
                EventId = createdEvent1.Id,
                EmployeeId = createdEmployee2.Id,
            };

            var insertQuery = @"
                INSERT INTO EmployeeEvent (EventId, EmployeeId)
                VALUES (@EventId, @EmployeeId);
            ";

            await _connection.ExecuteAsync(insertQuery, employeeEvent1);
            await _connection.ExecuteAsync(insertQuery, employeeEvent2);

            var result = await _context.EmployeeEvents.GetAllAsync(eventId: null, employeeId: createdEmployee1.Id, CancellationToken.None);

            result.ShouldHaveSingleItem();
            result.First().EmployeeId.ShouldBe(createdEmployee1.Id);
        }
        #endregion

        #region ExistsAsync
        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenEmployeeEventExists()
        {
            var event1 = new Event
            {
                Id = 0,
                Description = "Future Event",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now.AddDays(2),
                MaximumCapacity = 100
            };
            var createdEvent1 = await _context.Events.CreateAsync(event1, CancellationToken.None);

            var Employee1 = new Employee
            {
                Id = 0,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateOnly(1999, 1, 1)
            };
            var createdEmployee = await _context.Employees.CreateAsync(Employee1, CancellationToken.None);

            var employeeEvent = new EmployeeEvent
            {
                Id = 0,
                EventId = createdEvent1.Id,
                EmployeeId = createdEmployee.Id,
            };

            var insertQuery = @"
                INSERT INTO EmployeeEvent (EventId, EmployeeId)
                VALUES (@EventId, @EmployeeId)
                RETURNING ID;
            ";

            var id = await _connection.ExecuteAsync(insertQuery, employeeEvent);

            var exists = await _context.EmployeeEvents.ExistsAsync(id, CancellationToken.None);

            exists.ShouldBeTrue();
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenEmployeeEventDoesNotExist()
        {
            var nonExistentId = 999;

            var exists = await _context.EmployeeEvents.ExistsAsync(nonExistentId, CancellationToken.None);

            exists.ShouldBeFalse();
        }

        #endregion

        #region deleteAsync
        [Fact]
        public async Task DeleteAsync_ShouldSuccessfullyDeleteEvent_WhenEEmployeeEventExists()
        {
            var event1 = new Event
            {
                Id = 0,
                Description = "Future Event",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now.AddDays(2),
                MaximumCapacity = 100
            };
            var createdEvent1 = await _context.Events.CreateAsync(event1, CancellationToken.None);

            var Employee1 = new Employee
            {
                Id = 0,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateOnly(1999, 1, 1),
                FavouriteDrink = ""
            };
            var createdEmployee = await _context.Employees.CreateAsync(Employee1, CancellationToken.None);

            var employeeEvent = new EmployeeEvent
            {
                Id = 0,
                EventId = createdEvent1.Id,
                EmployeeId = createdEmployee.Id,
            };

            var insertQuery = @"
                INSERT INTO EmployeeEvent (EventId, EmployeeId)
                VALUES (@EventId, @EmployeeId)
                RETURNING ID;
            ";

            var id = await _connection.ExecuteAsync(insertQuery, employeeEvent);

            await _context.EmployeeEvents.DeleteAsync(id, CancellationToken.None);

            var eventAfterDelete = await _connection.QuerySingleOrDefaultAsync<Event>(
                "SELECT * FROM EmployeeEvent WHERE Id = @Id;", new { Id = id });

            eventAfterDelete.ShouldBeNull(); 
        }

        #endregion

        #region GetOneAsync
        [Fact]
        public async Task GetOneAsync_ShouldReturnEntries_WhenEmployeeEventIdIsSpecified()
        {
            var event1 = new Event
            {
                Id = 0,
                Description = "Future Event",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now.AddDays(2),
                MaximumCapacity = 100
            };
            var createdEvent1 = await _context.Events.CreateAsync(event1, CancellationToken.None);

            var Employee1 = new Employee
            {
                Id = 0,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateOnly(1999, 1, 1)
            };
            var createdEmployee = await _context.Employees.CreateAsync(Employee1, CancellationToken.None);

            var employeeEvent1 = new EmployeeEvent
            {
                Id = 0,
                EventId = createdEvent1.Id,
                EmployeeId = createdEmployee.Id,
            };

            var insertQuery = @"
                INSERT INTO EmployeeEvent (EventId, EmployeeId)
                VALUES (@EventId, @EmployeeId)
                RETURNING Id
            ";

            var id = await _connection.ExecuteAsync(insertQuery, employeeEvent1);

            var result = await _context.EmployeeEvents.GetOneAsync(id: id,  CancellationToken.None);

            result.EmployeeId.ShouldBe(createdEmployee.Id);
            result.EmployeeFirstName.ShouldBe(createdEmployee.FirstName);
            result.EmployeeLastName.ShouldBe(createdEmployee.LastName);
            result.EmployeeDateOfBirth.ShouldBe(createdEmployee.DateOfBirth);

            result.EventId.ShouldBe(createdEvent1.Id);
            result.EventDescription.ShouldBe(createdEvent1.Description);
            result.EventStartDateTime.ShouldBe(createdEvent1.StartDateTime);
            result.EventEndDateTime.ShouldBe(createdEvent1.EndDateTime);
            result.EventMaximumCapacity.ShouldBe(createdEvent1.MaximumCapacity);

        }
        #endregion
    }
}
