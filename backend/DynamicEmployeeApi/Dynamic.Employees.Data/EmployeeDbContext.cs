using Microsoft.EntityFrameworkCore;

namespace Dynamic.Employees.Data;

public class EmployeeDbContext(DbContextOptions<EmployeeDbContext> options) 
    : BaseEmployeeDbContext(options)
{
}
