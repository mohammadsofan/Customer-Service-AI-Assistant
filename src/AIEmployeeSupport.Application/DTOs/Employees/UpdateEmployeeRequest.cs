namespace AIEmployeeSupport.Application.DTOs.Employees;

public class UpdateEmployeeRequest
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
