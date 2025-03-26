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
    public class EmployeeRepositoryTests : TestBase
    {
        #region CreateTableIfNotExistsAsync
        [Fact]
        public async Task CreateTableIfNotExistsAsyncShould_CreateEmployeesTable()
        {
            await _connection.ExecuteAsync("DROP TABLE IF EXISTS Employee;"); //Drop table because table is automatically spun up on Context initialization
            await _context.Employees.CreateTableIfNotExistsAsync(CancellationToken.None);

            var tableExists = await _connection.ExecuteScalarAsync<int>(
                "SELECT count(*) FROM sqlite_master WHERE type='table' AND LOWER(name)='employee';"
            ) == 1;

            tableExists.ShouldBeTrue();
        }
        #endregion

        #region GetAllAsync
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllEmployees_WhenSearchTermIsNull()
        {
            var Employee1 = new Employee
            {
                Id = 0,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateOnly(1999, 1, 1)
            };
            var Employee2 = new Employee
            {
                Id = 0,
                FirstName = "Jane",
                LastName = "Doe",
                DateOfBirth = new DateOnly(2000, 1, 1)
            };
            var insertQuery = @"
            INSERT INTO Employee (FirstName, LastName, DateOfBirth)
            VALUES (@FirstName, @LastName, @DateOfBirth);
            ";

            await _connection.ExecuteAsync(insertQuery, Employee1);
            await _connection.ExecuteAsync(insertQuery, Employee2);

            var result = await _context.Employees.GetAllAsync( CancellationToken.None);

            result.Count.ShouldBe(2);
            result.Any(x => x.FirstName == Employee1.FirstName).ShouldBeTrue();
            result.Any(x => x.FirstName == Employee2.FirstName).ShouldBeTrue();
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ShouldReturnEmployee_WhenEmployeeExists()
        {
            var EmployeeToAdd = new Employee
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateOnly(1999, 1, 1)
            };

            var insertQuery = @"
            INSERT INTO Employee (FirstName, LastName, DateOfBirth)
            VALUES (@FirstName, @LastName, @DateOfBirth);
            ";

            await _connection.ExecuteAsync(insertQuery, EmployeeToAdd);

            var retrievedEmployee = await _context.Employees.GetByIdAsync(1, CancellationToken.None);

            retrievedEmployee.ShouldNotBeNull();
            retrievedEmployee.Id.ShouldBe(1);
            retrievedEmployee.FirstName.ShouldBe("John");
            retrievedEmployee.LastName.ShouldBe("Doe");
            retrievedEmployee.DateOfBirth.ShouldBe(EmployeeToAdd.DateOfBirth);

        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenEmployeeDoesNotExist()
        {
            var nonExistentId = 999;
            var retrievedEmployee = await _context.Employees.GetByIdAsync(nonExistentId, CancellationToken.None);

            retrievedEmployee.ShouldBeNull();
        }
        #endregion

        #region ExistsAsync
        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenEmployeeExists()
        {
            var EmployeeToAdd = new Employee
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateOnly(1999, 1, 1)
            };

            var insertQuery = @"
            INSERT INTO Employee (Id, FirstName, LastName, DateOfBirth)
            VALUES (@Id, @FirstName, @LastName, @DateOfBirth);
            ";

            await _connection.ExecuteAsync(insertQuery, EmployeeToAdd);

            var exists = await _context.Employees.ExistsAsync(1, CancellationToken.None);

            exists.ShouldBeTrue();
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenEmployeeDoesNotExist()
        {
            var nonExistentId = 999;

            var exists = await _context.Employees.ExistsAsync(nonExistentId, CancellationToken.None);

            exists.ShouldBeFalse();
        }

        #endregion

        #region CreateAsync
        [Fact]
        public async Task CreateAsync_ShouldReturnCreatedEmployee_WhenEmployeeIsValid()
        {
            var EmployeeToCreate = new Employee
            {
                Id = 0,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateOnly(1999, 1, 1),
                FavouriteDrink = "Mojito"
            };

            var createdEmployee = await _context.Employees.CreateAsync(EmployeeToCreate, CancellationToken.None);

            createdEmployee.ShouldNotBeNull();
            createdEmployee.Id.ShouldBeGreaterThan(0);
            createdEmployee.FirstName.ShouldBe("John");
            createdEmployee.LastName.ShouldBe("Doe");
            createdEmployee.DateOfBirth.ShouldBe(EmployeeToCreate.DateOfBirth);
            createdEmployee.FavouriteDrink.ShouldBe(EmployeeToCreate.FavouriteDrink);
        }

        [Fact]
        public async Task CreateAsync_ShouldInsertAnEmployeeWhenOtherEmployeesExist()
        {
            var Employee1 = new Employee
            {
                Id = 0,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateOnly(1999, 1, 1),
                FavouriteDrink = "Mojito"
            };

            var Employee2 = new Employee
            {
                Id = 0,
                FirstName = "Jane",
                LastName = "Doe",
                DateOfBirth = new DateOnly(2000, 1, 1),
                FavouriteDrink = "Guinness"
            };

            var createdEmployee1 = await _context.Employees.CreateAsync(Employee1, CancellationToken.None);
            var createdEmployee2 = await _context.Employees.CreateAsync(Employee2, CancellationToken.None);

            createdEmployee1.ShouldNotBeNull();
            createdEmployee1.Id.ShouldBeGreaterThan(0);
            createdEmployee1.FirstName.ShouldBe("John");
            createdEmployee1.LastName.ShouldBe("Doe");
            createdEmployee1.DateOfBirth.ShouldBe(Employee1.DateOfBirth);
            createdEmployee1.FavouriteDrink.ShouldBe(Employee1.FavouriteDrink);

            createdEmployee2.ShouldNotBeNull();
            createdEmployee2.Id.ShouldBeGreaterThan(0);
            createdEmployee2.FirstName.ShouldBe("Jane");
            createdEmployee2.LastName.ShouldBe("Doe");
            createdEmployee2.DateOfBirth.ShouldBe(Employee2.DateOfBirth);
            createdEmployee2.FavouriteDrink.ShouldBe(Employee2.FavouriteDrink);


            createdEmployee1.Id.ShouldNotBe(createdEmployee2.Id);
        }

        [Fact]
        public async Task CreateAsync_ShouldInsertEmployeeIntoDatabase()
        {
            // Arrange: Create an Employee
            var employeeToCreate = new Employee
            {
                Id = 0,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateOnly(1999, 1, 1),
                FavouriteDrink = "Mojito"
            };

            var createdEmployee = await _context.Employees.CreateAsync(employeeToCreate, CancellationToken.None);

            var insertedEmployee = await _connection.QuerySingleOrDefaultAsync<Employee>(
                "SELECT * FROM Employee WHERE Id = @Id;", new { createdEmployee.Id });

            insertedEmployee.ShouldNotBeNull();
            insertedEmployee.FirstName.ShouldBe("John");
            insertedEmployee.LastName.ShouldBe("Doe");
            createdEmployee.FavouriteDrink.ShouldBe(employeeToCreate.FavouriteDrink);
            insertedEmployee.DateOfBirth.ShouldBe(employeeToCreate.DateOfBirth);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ShouldReturnUpdatedEmployee_WhenEmployeeExists()
        {
            var EmployeeToInsert = new Employee
            {
                Id = 0,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateOnly(1999, 1, 1),
                FavouriteDrink = "Mojito"
            };

            var insertQuery = @"
            INSERT INTO Employee (FirstName, LastName, DateOfBirth, FavouriteDrink)
            VALUES (@FirstName, @LastName, @DateOfBirth, @FavouriteDrink)
            RETURNING Id;
            ";

            var insertedEmployee = await _connection.ExecuteScalarAsync<int>(insertQuery, EmployeeToInsert);

            var EmployeeToUpdate = new Employee
            {
                Id = insertedEmployee,
                FirstName = "Steve",
                LastName = "Jones",
                DateOfBirth = new DateOnly(1990, 1, 1),
                FavouriteDrink = "Guinness"
            };

            var updatedEmployee = await _context.Employees.UpdateAsync(EmployeeToUpdate, CancellationToken.None);

            updatedEmployee.FirstName.ShouldBe(EmployeeToUpdate.FirstName);
            updatedEmployee.LastName.ShouldBe(EmployeeToUpdate.LastName);
            updatedEmployee.DateOfBirth.ShouldBe(EmployeeToUpdate.DateOfBirth);
            updatedEmployee.FavouriteDrink.ShouldBe(EmployeeToUpdate.FavouriteDrink);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenEmployeeDoesNotExist()
        {
            var nonExistentEmployee = new Employee
            {
                Id = 99999,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateOnly(1999, 1, 1)
            };


            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _context.Employees.UpdateAsync(nonExistentEmployee, CancellationToken.None);
            });
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateEmployeeInDatabase()
        {
            var EmployeeToInsert = new Employee
            {
                Id=0,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateOnly(1999, 1, 1),
                FavouriteDrink = "Mojito"
            };

            var insertQuery = @"
            INSERT INTO Employee (FirstName, LastName, DateOfBirth, FavouriteDrink)
            VALUES (@FirstName, @LastName, @DateOfBirth, @FavouriteDrink)
            RETURNING Id;
        ";

            var insertedEmployeeId = await _connection.ExecuteScalarAsync<int>(insertQuery, EmployeeToInsert);

            var EmployeeToUpdate = new Employee
            {
                Id = insertedEmployeeId,
                FirstName = "Steve",
                LastName = "Jones",
                DateOfBirth = new DateOnly(1990, 1, 1),
                FavouriteDrink = "Guinness"
            };

            var updatedEmployee = await _context.Employees.UpdateAsync(EmployeeToUpdate, CancellationToken.None);

            var updatedEmployeeFromDb = await _connection.QuerySingleOrDefaultAsync<Employee>(
                "SELECT * FROM Employee WHERE Id = @Id;", new { updatedEmployee.Id });

            updatedEmployeeFromDb.ShouldNotBeNull();
            updatedEmployeeFromDb.FirstName.ShouldBe(EmployeeToUpdate.FirstName);
            updatedEmployeeFromDb.LastName.ShouldBe(EmployeeToUpdate.LastName);
            updatedEmployeeFromDb.DateOfBirth.ShouldBe(EmployeeToUpdate.DateOfBirth);
            updatedEmployeeFromDb.FavouriteDrink.ShouldBe(EmployeeToUpdate.FavouriteDrink);
        }

        [Fact]
        public async Task UpdateAsync_ShouldOnlyUpdateSpecifiedEmployee()
        {
            var EmployeeToUpdate = new Employee
            {
                Id = 0,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateOnly(1999, 1, 1),
                FavouriteDrink = "Mojito"
            };

            var EmployeeNotToUpdate = new Employee
            {
                Id = 0,
                FirstName = "Jane",
                LastName = "Doe",
                DateOfBirth = new DateOnly(2000, 1, 1),
                FavouriteDrink = "Orange Juice"
            };

            var insertQuery = @"
            INSERT INTO Employee (FirstName, LastName, DateOfBirth, FavouriteDrink)
            VALUES (@FirstName, @LastName, @DateOfBirth, @FavouriteDrink)
            RETURNING Id;
        ";

            var insertedEmployeeId1 = await _connection.ExecuteScalarAsync<int>(insertQuery, EmployeeToUpdate);
            var insertedEmployeeId2 = await _connection.ExecuteScalarAsync<int>(insertQuery, EmployeeNotToUpdate);

            var EmployeeToUpdateDto = new Employee
            {
                Id = insertedEmployeeId1,
                FirstName = "Steve",
                LastName = "Jones",
                DateOfBirth = new DateOnly(1990, 1, 1),
                FavouriteDrink = "Guinness"
            };

            var updatedEmployee = await _context.Employees.UpdateAsync(EmployeeToUpdateDto, CancellationToken.None);

            var updatedEmployeeFromDb = await _connection.QuerySingleOrDefaultAsync<Employee>(
                "SELECT * FROM Employee WHERE Id = @Id;", new { updatedEmployee.Id });

            updatedEmployeeFromDb.ShouldNotBeNull();
            updatedEmployeeFromDb.FirstName.ShouldBe(EmployeeToUpdateDto.FirstName);
            updatedEmployeeFromDb.LastName.ShouldBe(EmployeeToUpdateDto.LastName);
            updatedEmployeeFromDb.DateOfBirth.ShouldBe(EmployeeToUpdateDto.DateOfBirth);
            updatedEmployeeFromDb.FavouriteDrink.ShouldBe(EmployeeToUpdateDto.FavouriteDrink);

            //Other Employee should remain the same
            var notUpdatedEmployeeFromDb = await _connection.QuerySingleOrDefaultAsync<Employee>(
                "SELECT * FROM Employee WHERE Id = @Id;", new { Id = insertedEmployeeId2 });

            notUpdatedEmployeeFromDb.ShouldNotBeNull();
            notUpdatedEmployeeFromDb.FirstName.ShouldBe(EmployeeNotToUpdate.FirstName);
            notUpdatedEmployeeFromDb.LastName.ShouldBe(EmployeeNotToUpdate.LastName);
            notUpdatedEmployeeFromDb.DateOfBirth.ShouldBe(EmployeeNotToUpdate.DateOfBirth);
            notUpdatedEmployeeFromDb.FavouriteDrink.ShouldBe(EmployeeNotToUpdate.FavouriteDrink);
        }

        #endregion

        #region DeleteAsync
        [Fact]
        public async Task DeleteAsync_ShouldSuccessfullyDeleteEmployee_WhenEmployeeExists()
        {
            var EmployeeToInsert = new Employee
            {
                Id = 0,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateOnly(1999, 1, 1)
            };

            var insertQuery = @"
            INSERT INTO Employee (FirstName, LastName, DateOfBirth)
            VALUES (@FirstName, @LastName, @DateOfBirth)
            RETURNING Id;
        ";

            var insertedEmployeeId = await _connection.ExecuteScalarAsync<int>(insertQuery, EmployeeToInsert);

            await _context.Employees.DeleteAsync(insertedEmployeeId, CancellationToken.None);

            var EmployeeAfterDelete = await _connection.QuerySingleOrDefaultAsync<Employee>(
                "SELECT * FROM Employee WHERE Id = @Id;", new { Id = insertedEmployeeId });

            EmployeeAfterDelete.ShouldBeNull(); // Employee should not be found
        }

        #endregion
    }
}
