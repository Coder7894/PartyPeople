using System.ComponentModel;

namespace Website.DTOs
{
    public class EmployeeEventDTO
    {
        public int Id { get; set; }

        //Event
        public int EventId { get; set; }
        public string EventDescription { get; set; }
        public DateTime EventStartDateTime { get; init; }
        public DateTime EventEndDateTime { get; init; }
        public int? EventMaximumCapacity { get; init; }

        //Employee
        public int EmployeeId { get; set; }
        public string EmployeeFirstName { get; set; }
        public string EmployeeLastName { get; set; }
        public DateOnly EmployeeDateOfBirth { get; init; }
        public string EmployeeFavouriteDrink { get; init; }
    }
}
