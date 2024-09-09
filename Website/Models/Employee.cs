using System.ComponentModel;

namespace Website.Models;

/// <summary>
/// The employee model.
/// </summary>
public class Employee
{
    /// <summary>
    /// The unique identifier for this employee model.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// The first name for this employee model.
    /// </summary>
    [DisplayName("First Name")]
    public required string FirstName { get; init; }

    /// <summary>
    /// The last name for this employee model.
    /// </summary>
    [DisplayName("Last Name")]
    public required string LastName { get; init; }

    /// <summary>
    /// The date of birth for this employee model.
    /// </summary>
    [DisplayName("Date of Birth")]
    public required DateOnly DateOfBirth { get; init; }

    /// <summary>
    /// The favourite drink for this employee model.
    /// </summary>
    [DisplayName("Favourite Drink")]
    public string? FavouriteDrink { get; init; }
}