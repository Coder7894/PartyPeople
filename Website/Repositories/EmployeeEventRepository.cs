﻿using Dapper;
using System.Data;
using Website.Models;
using Website.Persistence;

namespace Website.Repositories;

/// <summary>
/// Repository for accessing employee event associations.
/// </summary>
public class EmployeeEventRepository : RepositoryBase
{
    /// <summary>
    /// Initialises a new instance of the <see cref="EmployeeEventRepository"/> class.
    /// </summary>
    /// <param name="connectionProvider">The connection provider to use.</param>
    public EmployeeEventRepository(IDbConnectionProvider connectionProvider) : base(connectionProvider)
    {
    }

    /// <summary>
    /// Creates the employee event table, if it doesn't already exist.
    /// </summary>
    /// <param name="cancellationToken">A token which can be used to cancel asynchronous operations.</param>
    /// <returns>An awaitable task</returns>
    public async Task CreateTableIfNotExistsAsync(CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(
            @"
                CREATE TABLE IF NOT EXISTS [EmployeeEvent] (
                    [EmployeeId] integer NOT NULL,
                    [EventId] integer NOT NULL,
                    PRIMARY KEY ([EmployeeId], [EventId]),
                    FOREIGN KEY ([EmployeeId]) REFERENCES [Employee]([Id]) ON DELETE CASCADE,
                    FOREIGN KEY ([EventId]) REFERENCES [Event]([Id]) ON DELETE CASCADE
                );
            ",
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        await Connection.ExecuteAsync(command);
    }

    /// <summary>
    /// Determines whether the employee event association exists.
    /// </summary>
    /// <param name="employeeId">The ID of the employee.</param>
    /// <param name="eventId">The ID of the event.</param>
    /// <param name="cancellationToken">A token which can be used to cancel asynchronous operations.</param>
    /// <returns>An awaitable task whose result is whether the association exists.</returns>
    public async ValueTask<bool> ExistsAsync(int employeeId, int eventId, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            @"
                SELECT  CAST(CASE
                     WHEN EXISTS (
                                     SELECT 1
                                        FROM   [EmployeeEvent] AS [EE]
                                        WHERE  [EE].[EmployeeId] = @EmployeeId
                                          AND  [EE].[EventId] = @EventId
                                 ) THEN 1
                     ELSE 0
                 END AS bit);
            ",
            parameters: new
            {
                EmployeeId = employeeId,
                EventId = eventId
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        return await Connection.ExecuteScalarAsync<bool>(command);
    }

    /// <summary>
    /// Creates a new employee event association.
    /// </summary>
    /// <param name="employeeEvent">The employee event association to create.</param>
    /// <returns>An awaitable task that results in the created employee, with its Id.</returns>
    public async Task<EmployeeEvent> CreateAsync(EmployeeEvent employeeEvent, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            @"
            INSERT INTO [EmployeeEvent]
            (
                [EmployeeId],
                [EventId]
            )
            VALUES
            (
                @EmployeeId,
                @EventId
            );

            SELECT  [E].[EmployeeId],
                    [E].[EventId]
            FROM    [EmployeeEvent] AS [E]
            WHERE   [E].[EmployeeId] = @EmployeeId
              AND   [E].[EventId] = @EventId;
        ",
            parameters: new
            {
                employeeEvent.EmployeeId,
                employeeEvent.EventId
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);
        
        return await Connection.QuerySingleAsync<EmployeeEvent>(command);
    }

    /// <summary>
    /// Deletes an existing employee event association.
    /// </summary>
    /// <param name="employeeId">The ID of the employee.</param>
    /// <param name="eventId">The ID of the event.</param>
    /// <param name="cancellationToken">A token which can be used to cancel asynchronous operations.</param>
    /// <returns>An awaitable task.</returns>
    public async Task DeleteAsync(int employeeId, int eventId, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            @"
                DELETE FROM [EmployeeEvent]
                WHERE [EmployeeId] = @EmployeeId
                  AND [EventId] = @EventId;
            ",
            parameters: new
            {
                EmployeeId = employeeId,
                EventId = eventId
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        await Connection.ExecuteAsync(command);
    }

    /// <summary>
    /// Gets the coun of employees attending each event.
    /// <param name="eventIds">The IDs of the events to get the count of employees for.</param>
    /// <param name="cancellationToken">A token which can be used to cancel asynchronous operations.</param>
    /// <returns>An awaitable task whose result is a dictionary of event IDs with their employee count.</returns>
    /// </summary>
    public async Task<Dictionary<int, int>> GetEmployeesAtEventCountAsync(IEnumerable<int> eventIds, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            @"
                SELECT [E].[EventId],
                       COUNT([E].[EmployeeId]) AS [EmployeeCount]
                FROM [EmployeeEvent] AS [E]
                WHERE [E].[EventId] IN @EventIds
                GROUP BY [E].[EventId];
            ",
            parameters: new
            {
                EventIds = eventIds
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        var eventEmployeeCounts = await Connection.QueryAsync<(int EventId, int EmployeeCount)>(command);

        // Convert the result to a dictionary
        return eventEmployeeCounts.ToDictionary(x => x.EventId, x => x.EmployeeCount);
    }

    // Next is getting the actual employees at an event
    /// <summary>
    /// Gets the employees attending an event.
    /// </summary>
    /// <param name="eventId">The ID of the event to get the employees for.</param>
    /// <param name="cancellationToken">A token which can be used to cancel asynchronous operations.</param>
    /// <returns>An awaitable task whose result is the employees attending the event.</returns>
    public async Task<IReadOnlyCollection<Employee>> GetEmployeesAtEventAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            @"
                SELECT  [E].[Id],
                        [E].[FirstName],
                        [E].[LastName],
                        [E].[DateOfBirth]
                FROM    [Employee] AS [E]
                JOIN    [EmployeeEvent] AS [EE]
                ON      [E].[Id] = [EE].[EmployeeId]
                WHERE   [EE].[EventId] = @EventId;
            ",
            parameters: new
            {
                EventId = eventId
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        var employees = await Connection.QueryAsync<Employee>(command);
        return employees
            .OrderBy(x => x.LastName)
            .ToArray();
    }
}