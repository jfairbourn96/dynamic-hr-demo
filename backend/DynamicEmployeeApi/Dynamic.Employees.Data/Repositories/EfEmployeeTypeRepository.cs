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
    public async Task<List<EmployeeType>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.EmployeeTypes.ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<EmployeeType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.EmployeeTypes.FirstOrDefaultAsync(et => et.Id == id, cancellationToken);

    /// <inheritdoc/>
    public async Task AddAsync(EmployeeType employeeType, CancellationToken cancellationToken = default)
    {
        await context.EmployeeTypes.AddAsync(employeeType, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(EmployeeType employeeType, CancellationToken cancellationToken = default)
    {
        context.EmployeeTypes.Update(employeeType);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(EmployeeType employeeType, CancellationToken cancellationToken = default)
    {
        context.EmployeeTypes.Remove(employeeType);
        await context.SaveChangesAsync(cancellationToken);
    }
}
