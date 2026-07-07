using Dynamic.Employees.Application.Interfaces;
using Dynamic.Employees.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Dynamic.Employees.Data.Repositories;

/// <inheritdoc/>
public class EfEmployeeTypeRepository(BaseEmployeeDbContext context) :
    IEmployeeTypeReader,
    IEmployeeTypeWriter
{
    /// <inheritdoc/>
    public async Task<List<EmployeeType>> GetAllAsync() => await context.EmployeeTypes.ToListAsync();

    /// <inheritdoc/>
    public async Task<EmployeeType?> GetByIdAsync(Guid id)
        => await context.EmployeeTypes.FirstOrDefaultAsync(et => et.Id == id);

    /// <inheritdoc/>
    public async Task AddAsync(EmployeeType employeeType)
    {
        await context.EmployeeTypes.AddAsync(employeeType);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(EmployeeType employeeType)
    {
        context.EmployeeTypes.Update(employeeType);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(EmployeeType employeeType)
    {
        context.EmployeeTypes.Remove(employeeType);
        await context.SaveChangesAsync();
    }
}
