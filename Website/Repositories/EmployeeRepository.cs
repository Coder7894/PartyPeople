using Dapper;
using System.Data;
using Website.Models;
using Website.Persistence;

namespace Website.Repositories;

/// <summary>
/// Repository for accessing employees from a database.
/// </summary>
public class EmployeeRepository : RepositoryBase
{
    /// <summary>
    /// Initialises a new instance of the <see cref="EmployeeRepository"/> class.
    /// </summary>
    /// <param name="connectionProvider">The connection provider to use.</param>
    public EmployeeRepository(IDbConnectionProvider connectionProvider) : base(connectionProvider)
    {
    }

    /// <summary>
    /// Creates the employee table, if it doesn't already exist.
    /// </summary>
    /// <param name="cancellationToken">A token which can be used to cancel asynchronous operations.</param>
    /// <returns>An awaitable task</returns>
    public async Task CreateTableIfNotExistsAsync(CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(
            @"
                CREATE TABLE IF NOT EXISTS [Employee] (
                    [Id] integer primary key,
                    [FirstName] nvarchar(2147483647) NOT NULL COLLATE NOCASE,
                    [LastName] nvarchar(2147483647) NOT NULL COLLATE NOCASE,
                    [DateOfBirth] date NOT NULL,
                    [FavouriteDrink] nvarchar(2147483647) NULL COLLATE NOCASE
                );
            ",
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        await Connection.ExecuteAsync(command);
    }

    /// <summary>
    /// Gets all employees.
    /// </summary>
    /// <param name="cancellationToken">A token which can be used to cancel asynchronous operations.</param>
    /// <returns>An awaitable task whose result is the employees found.</returns>
    public async Task<IReadOnlyCollection<Employee>> GetAllAsync(CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(
            @"
                SELECT  [Id],
                        [FirstName],
                        [LastName],
                        [DateOfBirth],
                        [FavouriteDrink]
                FROM    [Employee] AS [E];
            ",
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        var employees = await Connection.QueryAsync<Employee>(command);

        return employees
            .OrderBy(x => x.LastName)
            .ToArray();
    }

    /// <summary>
    /// Gets an employee by ID.
    /// </summary>
    /// <param name="id">The ID of the employee to get.</param>
    /// <param name="cancellationToken">A token which can be used to cancel asynchronous operations.</param>
    /// <returns>An awaitable task whose result is the employee if found, otherwise <see langword="null"/>.</returns>
    public async ValueTask<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            @"
                SELECT  [E].[Id],
                        [E].[FirstName],
                        [E].[LastName],
                        [E].[DateOfBirth],
                        [E].[FavouriteDrink]
                FROM    [Employee] AS [E]
                WHERE   [E].[Id] = @Id;
            ",
            parameters: new
            {
                Id = id
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        return await Connection.QuerySingleOrDefaultAsync<Employee>(command);
    }

    /// <summary>
    /// Determines whether an employee with the given ID exists.
    /// </summary>
    /// <param name="id">The ID of the employee to check.</param>
    /// <param name="cancellationToken">A token which can be used to cancel asynchronous operations.</param>
    /// <returns>An awaitable task whose result indicates whether the employee exists.</returns>
    public async ValueTask<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            @"
                SELECT  CAST(CASE
                     WHEN EXISTS (
                                     SELECT 1
                                     FROM   [Employee] AS [E]
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
    /// Creates a new employee.
    /// </summary>
    /// <param name="employee">The employee to create. The <see cref="Employee.Id"/> is ignored.</param>
    /// <returns>An awaitable task that results in the created employee, with its Id.</returns>
    public async Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            @"
                INSERT INTO [Employee]
                (
                    [FirstName],
                    [LastName],
                    [DateOfBirth],
                    [FavouriteDrink]
                )
                VALUES
                (
                    @FirstName,
                    @LastName,
                    @DateOfBirth,
                    @FavouriteDrink
                );

                SELECT  [E].[Id],
                        [E].[FirstName],
                        [E].[LastName],
                        [E].[DateOfBirth],
                        [E].[FavouriteDrink]
                FROM    [Employee] AS [E]
                WHERE   [E].[Id] = last_insert_rowid();
            ",
            parameters: new
            {
                employee.FirstName,
                employee.LastName,
                employee.DateOfBirth,
                employee.FavouriteDrink
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        var createdEmployee = await Connection.QuerySingleAsync<Employee>(command);

        return createdEmployee;
    }

    /// <summary>
    /// Updates an existing employee.
    /// </summary>
    /// <param name="employee">The updated employee details. The <see cref="Employee.Id"/> should be the Id of the employee to update.</param>
    /// <returns>An awaitable task that results in the updated employee.</returns>
    public async Task<Employee> UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            @"
                UPDATE  [Employee]
                SET     [FirstName] = @FirstName,
                        [LastName] = @LastName,
                        [DateOfBirth] = @DateOfBirth,
                        [FavouriteDrink] = @FavouriteDrink
                WHERE   [Id] = @Id;

                SELECT  [E].[Id],
                        [E].[FirstName],
                        [E].[LastName],
                        [E].[DateOfBirth],
                        [E].[FavouriteDrink]
                FROM    [Employee] AS [E]
                WHERE   [E].[Id] = @Id;
            ",
            parameters: new
            {
                employee.Id,
                employee.FirstName,
                employee.LastName,
                employee.DateOfBirth,
                employee.FavouriteDrink
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        var updatedEmployee = await Connection.QuerySingleAsync<Employee>(command);

        return updatedEmployee;
    }

    /// <summary>
    /// Deletes an existing employee.
    /// </summary>
    /// <param name="employeeId">The ID of the employee to delete.</param>
    /// <returns>An awaitable task.</returns>
    public async Task DeleteAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            @"
                DELETE FROM [Employee]
                WHERE   [Id] = @Id;
            ",
            parameters: new
            {
                Id = employeeId
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        await Connection.ExecuteAsync(command);
    }
}