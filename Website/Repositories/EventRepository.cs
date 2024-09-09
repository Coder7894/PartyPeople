using Dapper;
using System.Data;
using Website.Models;
using Website.Persistence;

namespace Website.Repositories;

/// <summary>
/// Repository for accessing events from a database.
/// </summary>
public class EventRepository : RepositoryBase
{
    /// <summary>
    /// Initialises a new instance of the <see cref="EventRepository"/> class.
    /// </summary>
    /// <param name="connectionProvider">The connection provider to use.</param>
    public EventRepository(IDbConnectionProvider connectionProvider) : base(connectionProvider)
    {
    }

    /// <summary>
    /// Creates the event table, if it doesn't already exist.
    /// </summary>
    /// <param name="cancellationToken">A token which can be used to cancel asynchronous operations.</param>
    /// <returns>An awaitable task</returns>
    public async Task CreateTableIfNotExistsAsync(CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(
            @"
                CREATE TABLE IF NOT EXISTS [Event] (                    
                    [Id] integer primary key,
                    [Description] nvarchar(2147483647) NOT NULL COLLATE NOCASE,
                    [StartDateTime] datetime NOT NULL,
                    [EndDateTime] datetime NOT NULL,
                    [MaximumCapacity] int NULL
                );
            ",
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        await Connection.ExecuteAsync(command);
    }

    /// <summary>
    /// Gets all events.
    /// </summary>
    /// <param name="cancellationToken">A token which can be used to cancel asynchronous operations.</param>
    /// <returns>An awaitable task whose result is the events found.</returns>
    public async Task<IReadOnlyCollection<Event>> GetAllAsync(bool includeHistoricEvents = false, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            @"
                SELECT  [E].[Id],
                        [E].[Description],
                        [E].[StartDateTime],
                        [E].[EndDateTime],
                        [E].[MaximumCapacity]
                FROM    [Event] AS [E]
                WHERE   (
                            @IncludeHistoricEvents = 1
                            OR  [E].[EndDateTime] > DATE('now')
                        );
            ",
            parameters: new
            {
                IncludeHistoricEvents = includeHistoricEvents
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        var events = await Connection.QueryAsync<Event>(command);

        return events
            .OrderBy(x => x.StartDateTime)
            .ToArray();
    }

    /// <summary>
    /// Gets an event by ID.
    /// </summary>
    /// <param name="id">The ID of the event to get.</param>
    /// <param name="cancellationToken">A token which can be used to cancel asynchronous operations.</param>
    /// <returns>An awaitable task whose result is the event if found, otherwise <see langword="null"/>.</returns>
    public async ValueTask<Event?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            @"
                SELECT  [E].[Id],
                        [E].[Description],
                        [E].[StartDateTime],
                        [E].[EndDateTime],
                        [E].[MaximumCapacity]
                FROM    [Event] AS [E]
                WHERE   [E].[Id] = @Id;
            ",
            parameters: new
            {
                Id = id
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        return await Connection.QuerySingleOrDefaultAsync<Event>(command);
    }

    /// <summary>
    /// Determines whether an event with the given ID exists.
    /// </summary>
    /// <param name="id">The ID of the event to check.</param>
    /// <param name="cancellationToken">A token which can be used to cancel asynchronous operations.</param>
    /// <returns>An awaitable task whose result indicates whether the event exists.</returns>
    public async ValueTask<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            @"SELECT  CAST(CASE
                     WHEN EXISTS (
                                     SELECT 1
                                     FROM   [Event] AS [E]
                                     WHERE  [E].Id = @Id
                                 ) THEN 1
                     ELSE 0
                 END AS bit);
            ",
            parameters: new
            {
                Id = id
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        return await Connection.ExecuteScalarAsync<bool>(command);
    }

    /// <summary>
    /// Creates a new event.
    /// </summary>
    /// <param name="event">The event to create. The <see cref="Event.Id"/> is ignored.</param>
    /// <returns>An awaitable task that results in the created event, with its Id.</returns>
    public async Task<Event> CreateAsync(Event @event, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            @"
                INSERT INTO [Event]
                (
                    [Description],
                    [StartDateTime],
                    [EndDateTime],
                    [MaximumCapacity]
                )
                VALUES
                (
                    @Description,
                    @StartDateTime,
                    @EndDateTime,
                    @MaximumCapacity
                );

                SELECT  [E].[Id],
                        [E].[Description],
                        [E].[StartDateTime],
                        [E].[EndDateTime],
                        [E].[MaximumCapacity]
                FROM    [Event] AS [E]
                WHERE   [E].[Id] = last_insert_rowid();
            ",
            parameters: new
            {
                @event.Description,
                @event.StartDateTime,
                @event.EndDateTime,
                @event.MaximumCapacity
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        var createdEvent = await Connection.QuerySingleAsync<Event>(command);

        return createdEvent;
    }

    /// <summary>
    /// Updates an existing event.
    /// </summary>
    /// <param name="event">The updated event details. The <see cref="Event.Id"/> should be the Id of the event to update.</param>
    /// <returns>An awaitable task that results in the updated event.</returns>
    public async Task<Event> UpdateAsync(Event @event, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            @"
                UPDATE  [Event]
                SET     [Description] = @Description,
                        [StartDateTime] = @StartDateTime,
                        [EndDateTime] = @EndDateTime,
                        [MaximumCapacity] = @MaximumCapacity
                WHERE   [Id] = @Id;

                SELECT  [E].[Id],
                        [E].[Description],
                        [E].[StartDateTime],
                        [E].[EndDateTime],
                        [E].[MaximumCapacity]
                FROM    [Event] AS [E]
                WHERE   [E].[Id] = @Id;
            ",
            parameters: new
            {
                @event.Id,
                @event.Description,
                @event.StartDateTime,
                @event.EndDateTime,
                @event.MaximumCapacity
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        var updatedEvent = await Connection.QuerySingleAsync<Event>(command);

        return updatedEvent;
    }

    /// <summary>
    /// Deletes an existing event.
    /// </summary>
    /// <param name="eventId">The ID of the event to delete.</param>
    /// <returns>An awaitable task.</returns>
    public async Task DeleteAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            @"
                DELETE FROM [Event]
                WHERE   [Id] = @Id;
            ",
            parameters: new
            {
                Id = eventId
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        await Connection.ExecuteAsync(command);
    }
}