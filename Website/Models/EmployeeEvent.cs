namespace Website.Models
{
    public class EmployeeEvent
    {
        /// <summary>
        /// The unique identifier for this employee model.
        /// </summary>
        public required int Id { get; init; }

        /// <summary>
        /// The identifier of the employee
        /// </summary>
        public required int EmployeeId { get; init; }

        /// <summary>
        /// The model of the employee
        /// </summary>
        public Employee? Employee { get; init; }

        /// <summary>
        /// The identifier of the event
        /// </summary>
        public required int EventId { get; init; }

        /// <summary>
        /// The model of the event
        /// </summary>
        public Employee? Event { get; init; }
    }
}
