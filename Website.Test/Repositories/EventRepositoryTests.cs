using Dapper;
using Microsoft.AspNetCore.Connections;
using NuGet.Protocol.Core.Types;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Models;
using Website.Repositories;

namespace Website.Test.Repositories
{
    public class EventRepositoryTests : TestBase
    {
        #region CreateTableIfNotExistsAsync
        [Fact]
        public async Task CreateTableIfNotExistsAsyncShould_CreateEventsTable()
        {
            await _connection.ExecuteAsync("DROP TABLE IF EXISTS Event;"); //Drop table because table is automatically spun up on Context initialization
            await _context.Events.CreateTableIfNotExistsAsync(CancellationToken.None);

            var tableExists = await _connection.ExecuteScalarAsync<int>(
                "SELECT count(*) FROM sqlite_master WHERE type='table' AND LOWER(name)='event';"
            ) == 1;

            tableExists.ShouldBeTrue();
        }
        #endregion

        #region GetAllAsync
        [Fact]
        public async Task GetAllAsync_ShouldReturnOnlyFutureEvents_WhenIncludeHistoricEventsIsFalse()
        {
            var event1 = new Event
            {
                Id = 0,
                Description = "Future Event",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now.AddDays(2),
                MaximumCapacity = 100
            };
            var event2 = new Event
            {
                Id = 0,
                Description = "Past Event",
                StartDateTime = DateTime.Now.AddDays(-1),
                EndDateTime = DateTime.Now.AddDays(-1),
                MaximumCapacity = 100
            };
            var insertQuery = @"
            INSERT INTO Event (Description, StartDateTime, EndDateTime, MaximumCapacity)
            VALUES (@Description, @StartDateTime, @EndDateTime, @MaximumCapacity);
            ";

            await _connection.ExecuteAsync(insertQuery, event1);
            await _connection.ExecuteAsync(insertQuery, event2);

            var result = await _context.Events.GetAllAsync(includeHistoricEvents: false, CancellationToken.None);

            result.ShouldHaveSingleItem();
            result.First().Description.ShouldBe("Future Event");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllEvents_WhenIncludeHistoricEventsIsTrue()
        {
            var event1 = new Event
            {
                Id = 0,
                Description = "Future Event",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now.AddDays(2),
                MaximumCapacity = 100
            };
            var event2 = new Event
            {
                Id = 0,
                Description = "Past Event",
                StartDateTime = DateTime.Now.AddDays(-1),
                EndDateTime = DateTime.Now.AddDays(-1),
                MaximumCapacity = 100
            };
            var insertQuery = @"
            INSERT INTO Event (Description, StartDateTime, EndDateTime, MaximumCapacity)
            VALUES (@Description, @StartDateTime, @EndDateTime, @MaximumCapacity);
            ";

            await _connection.ExecuteAsync(insertQuery, event1);
            await _connection.ExecuteAsync(insertQuery, event2);

            var result = await _context.Events.GetAllAsync(includeHistoricEvents: true, CancellationToken.None);

            result.Count.ShouldBe(2);
            result.Any(x => x.Description == "Future Event").ShouldBeTrue();
            result.Any(x => x.Description == "Past Event").ShouldBeTrue();
        }
        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ShouldReturnEvent_WhenEventExists()
        {
            var eventToAdd = new Event
            {
                Id = 0,
                Description = "Future Event",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now.AddDays(2),
                MaximumCapacity = 100
            };

            var insertQuery = @"
            INSERT INTO Event (Description, StartDateTime, EndDateTime, MaximumCapacity)
            VALUES (@Description, @StartDateTime, @EndDateTime, @MaximumCapacity)
            RETURNING Id;
            ";

            var id = await _connection.ExecuteAsync(insertQuery, eventToAdd);

            var retrievedEvent = await _context.Events.GetByIdAsync(id, CancellationToken.None);

            retrievedEvent.ShouldNotBeNull();
            retrievedEvent.Id.ShouldBe(1);
            retrievedEvent.Description.ShouldBe("Future Event");

        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenEventDoesNotExist()
        {
            var nonExistentId = 999;
            var retrievedEvent = await _context.Events.GetByIdAsync(nonExistentId, CancellationToken.None);

            retrievedEvent.ShouldBeNull();
        }
        #endregion

        #region ExistsAsync
        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenEventExists()
        {
            var eventToAdd = new Event
            {
                Id = 0,
                Description = "Future Event",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now.AddDays(2),
                MaximumCapacity = 100
            };

            var insertQuery = @"
            INSERT INTO Event (Description, StartDateTime, EndDateTime, MaximumCapacity)
            VALUES (@Description, @StartDateTime, @EndDateTime, @MaximumCapacity)
            RETURNING Id;
            ";

            var id = await _connection.ExecuteAsync(insertQuery, eventToAdd);

            var exists = await _context.Events.ExistsAsync(id, CancellationToken.None);

            exists.ShouldBeTrue();
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenEventDoesNotExist()
        {
            var nonExistentId = 999;

            var exists = await _context.Events.ExistsAsync(nonExistentId, CancellationToken.None);

            exists.ShouldBeFalse();
        }

        #endregion

        #region CreateAsync
        [Fact]
        public async Task CreateAsync_ShouldReturnCreatedEvent_WhenEventIsValid()
        {
            var eventToCreate = new Event
            {
                Id = 0,
                Description = "New Event",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now.AddDays(2),
                MaximumCapacity = 200
            };

            var createdEvent = await _context.Events.CreateAsync(eventToCreate, CancellationToken.None);

            createdEvent.ShouldNotBeNull();
            createdEvent.Id.ShouldBeGreaterThan(0);
            createdEvent.Description.ShouldBe("New Event");
            createdEvent.StartDateTime.ShouldBe(eventToCreate.StartDateTime);
            createdEvent.EndDateTime.ShouldBe(eventToCreate.EndDateTime);
            createdEvent.MaximumCapacity.ShouldBe(eventToCreate.MaximumCapacity);
        }

        [Fact]
        public async Task CreateAsync_ShouldInsertAnEventWhenOtherEventsExist()
        {
            var event1 = new Event
            {
                Id = 0,
                Description = "First Event",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now.AddDays(2),
                MaximumCapacity = 100
            };

            var event2 = new Event
            {
                Id = 0,
                Description = "Second Event",
                StartDateTime = DateTime.Now.AddDays(3),
                EndDateTime = DateTime.Now.AddDays(4),
                MaximumCapacity = 150
            };

            var createdEvent1 = await _context.Events.CreateAsync(event1, CancellationToken.None);
            var createdEvent2 = await _context.Events.CreateAsync(event2, CancellationToken.None);

            createdEvent1.ShouldNotBeNull();
            createdEvent1.Id.ShouldBeGreaterThan(0);
            createdEvent1.Description.ShouldBe("First Event");

            createdEvent2.ShouldNotBeNull();
            createdEvent2.Id.ShouldBeGreaterThan(0);
            createdEvent2.Description.ShouldBe("Second Event");

            createdEvent1.Id.ShouldNotBe(createdEvent2.Id);
        }

        [Fact]
        public async Task CreateAsync_ShouldInsertEventIntoDatabase()
        {
            // Arrange: Create an event
            var eventToCreate = new Event
            {
                Id = 0,
                Description = "Database Insert Test Event",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now.AddDays(2),
                MaximumCapacity = 250
            };

            var createdEvent = await _context.Events.CreateAsync(eventToCreate, CancellationToken.None);

            var insertedEvent = await _connection.QuerySingleOrDefaultAsync<Event>(
                "SELECT * FROM Event WHERE Id = @Id;", new { createdEvent.Id });

            insertedEvent.ShouldNotBeNull();
            insertedEvent.Description.ShouldBe("Database Insert Test Event");
            insertedEvent.StartDateTime.ShouldBe(eventToCreate.StartDateTime);
            insertedEvent.EndDateTime.ShouldBe(eventToCreate.EndDateTime);
            insertedEvent.MaximumCapacity.ShouldBe(eventToCreate.MaximumCapacity);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ShouldReturnUpdatedEvent_WhenEventExists()
        {
            var eventToInsert = new Event
            {
                Id = 0,
                Description = "Old Event Description",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now.AddDays(2),
                MaximumCapacity = 100
            };

            var insertQuery = @"
            INSERT INTO Event (Description, StartDateTime, EndDateTime, MaximumCapacity)
            VALUES (@Description, @StartDateTime, @EndDateTime, @MaximumCapacity)
            RETURNING Id;
            ";

            var insertedEvent = await _connection.ExecuteScalarAsync<int>(insertQuery, eventToInsert);

            var eventToUpdate = new Event
            {
                Id = insertedEvent,
                Description = "Updated Event Description",
                StartDateTime = DateTime.Now.AddDays(3),
                EndDateTime = DateTime.Now.AddDays(4),
                MaximumCapacity = 150
            };

            var updatedEvent = await _context.Events.UpdateAsync(eventToUpdate, CancellationToken.None);

            updatedEvent.Description.ShouldBe("Updated Event Description");
            updatedEvent.StartDateTime.ShouldBe(eventToUpdate.StartDateTime);
            updatedEvent.EndDateTime.ShouldBe(eventToUpdate.EndDateTime);
            updatedEvent.MaximumCapacity.ShouldBe(eventToUpdate.MaximumCapacity);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenEventDoesNotExist()
        {
            var nonExistentEvent = new Event
            {
                Id = 99999,
                Description = "Non-Existent Event",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now.AddDays(2),
                MaximumCapacity = 100
            };


            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _context.Events.UpdateAsync(nonExistentEvent, CancellationToken.None);
            });
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateEventInDatabase()
        {
            var eventToInsert = new Event
            {
                Id=0,
                Description = "Old Event Description",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now.AddDays(2),
                MaximumCapacity = 100
            };

            var insertQuery = @"
            INSERT INTO Event (Description, StartDateTime, EndDateTime, MaximumCapacity)
            VALUES (@Description, @StartDateTime, @EndDateTime, @MaximumCapacity)
            RETURNING Id;
        ";

            var insertedEventId = await _connection.ExecuteScalarAsync<int>(insertQuery, eventToInsert);

            var eventToUpdate = new Event
            {
                Id = insertedEventId,
                Description = "Updated Event Description",
                StartDateTime = DateTime.Now.AddDays(3),
                EndDateTime = DateTime.Now.AddDays(4),
                MaximumCapacity = 150
            };

            var updatedEvent = await _context.Events.UpdateAsync(eventToUpdate, CancellationToken.None);

            var updatedEventFromDb = await _connection.QuerySingleOrDefaultAsync<Event>(
                "SELECT * FROM Event WHERE Id = @Id;", new { updatedEvent.Id });

            updatedEventFromDb.ShouldNotBeNull();
            updatedEventFromDb.Description.ShouldBe("Updated Event Description");
            updatedEventFromDb.StartDateTime.ShouldBe(eventToUpdate.StartDateTime);
            updatedEventFromDb.EndDateTime.ShouldBe(eventToUpdate.EndDateTime);
            updatedEventFromDb.MaximumCapacity.ShouldBe(eventToUpdate.MaximumCapacity);
        }

        [Fact]
        public async Task UpdateAsync_ShouldOnlyUpdateSpecifiedEvent()
        {
            var eventToUpdate = new Event
            {
                Id = 0,
                Description = "Event to update",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now.AddDays(2),
                MaximumCapacity = 100
            };

            var eventNotToUpdate = new Event
            {
                Id = 0,
                Description = "Event not to update",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now.AddDays(2),
                MaximumCapacity = 100
            };

            var insertQuery = @"
            INSERT INTO Event (Description, StartDateTime, EndDateTime, MaximumCapacity)
            VALUES (@Description, @StartDateTime, @EndDateTime, @MaximumCapacity)
            RETURNING Id;
        ";

            var insertedEventId1 = await _connection.ExecuteScalarAsync<int>(insertQuery, eventToUpdate);
            var insertedEventId2 = await _connection.ExecuteScalarAsync<int>(insertQuery, eventNotToUpdate);

            var eventToUpdateDto = new Event
            {
                Id = insertedEventId1,
                Description = "Updated Event Description",
                StartDateTime = DateTime.Now.AddDays(3),
                EndDateTime = DateTime.Now.AddDays(4),
                MaximumCapacity = 150
            };

            var updatedEvent = await _context.Events.UpdateAsync(eventToUpdateDto, CancellationToken.None);

            var updatedEventFromDb = await _connection.QuerySingleOrDefaultAsync<Event>(
                "SELECT * FROM Event WHERE Id = @Id;", new { updatedEvent.Id });

            updatedEventFromDb.ShouldNotBeNull();
            updatedEventFromDb.Description.ShouldBe("Updated Event Description");
            updatedEventFromDb.StartDateTime.ShouldBe(eventToUpdateDto.StartDateTime);
            updatedEventFromDb.EndDateTime.ShouldBe(eventToUpdateDto.EndDateTime);
            updatedEventFromDb.MaximumCapacity.ShouldBe(eventToUpdateDto.MaximumCapacity);

            //Other event should remain the same
            var notUpdatedEventFromDb = await _connection.QuerySingleOrDefaultAsync<Event>(
                "SELECT * FROM Event WHERE Id = @Id;", new { Id = insertedEventId2 });

            notUpdatedEventFromDb.ShouldNotBeNull();
            notUpdatedEventFromDb.Description.ShouldBe(eventNotToUpdate.Description);
            notUpdatedEventFromDb.StartDateTime.ShouldBe(eventNotToUpdate.StartDateTime);
            notUpdatedEventFromDb.EndDateTime.ShouldBe(eventNotToUpdate.EndDateTime);
            notUpdatedEventFromDb.MaximumCapacity.ShouldBe(eventNotToUpdate.MaximumCapacity);
        }

        #endregion

        #region deleteAsync
        [Fact]
        public async Task DeleteAsync_ShouldSuccessfullyDeleteEvent_WhenEventExists()
        {
            var eventToInsert = new Event
            {
                Id = 0,
                Description = "Event to Delete",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now.AddDays(2),
                MaximumCapacity = 100
            };

            var insertQuery = @"
            INSERT INTO Event (Description, StartDateTime, EndDateTime, MaximumCapacity)
            VALUES (@Description, @StartDateTime, @EndDateTime, @MaximumCapacity)
            RETURNING Id;
        ";

            var insertedEventId = await _connection.ExecuteScalarAsync<int>(insertQuery, eventToInsert);

            await _context.Events.DeleteAsync(insertedEventId, CancellationToken.None);

            var eventAfterDelete = await _connection.QuerySingleOrDefaultAsync<Event>(
                "SELECT * FROM Event WHERE Id = @Id;", new { Id = insertedEventId });

            eventAfterDelete.ShouldBeNull(); // Event should not be found
        }

        #endregion
    }
}
