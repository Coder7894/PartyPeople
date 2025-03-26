using System.ComponentModel;

namespace Website.DTOs
{
    public class EmployeeEventCountDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int EventCount { get; set; }
    }
}
