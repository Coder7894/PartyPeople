﻿namespace Website.Models;

public class EventEditViewModel
{
    public required Event Event { get; init; }
    public IReadOnlyCollection<Employee>? Employees { get; set; }
    public required Dictionary<int, bool> EmployeeAttendance { get; init; }
}
