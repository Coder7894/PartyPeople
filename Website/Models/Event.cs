using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Website.DTOs;

namespace Website.Models;

/// <summary>
/// The Event model.
/// </summary>
public class Event
{
    /// <summary>
    /// The unique identifier of this event model.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// The description for this event model.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// The date and time this event will start.
    /// </summary>
    [DisplayName("Event Start")]
    public required DateTime StartDateTime { get; init; }

    /// <summary>
    /// The date and time this event will finish.
    /// </summary>
    [DisplayName("Event Finish")]
    public required DateTime EndDateTime { get; init; }

    /// <summary>
    /// The maximum capacity for this event.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, the event can be attended by an unlimited number of employees.
    /// </remarks>
    [DisplayName("Maximum Capacity")]
    public int? MaximumCapacity { get; init; }

    /// <summary>
    /// A list of employess attending this event
    /// </summary>
    [NotMapped]
    public List<EmployeeEventDTO> Employees = new List<EmployeeEventDTO>();

    [NotMapped]
    public bool AtCapacity => Employees.Count >= MaximumCapacity;
}