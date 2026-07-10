using Dynamic.Employees.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dynamic.Employees.Data.Configurations;

/// <summary>
/// Configures persistence for employee type entities.
/// </summary>
/// <remarks>
/// Field definitions and select options are owned JSON collections because they form the runtime
/// schema of one employee type and are replaced as part of that aggregate rather than queried independently.
/// </remarks>
public class EmployeeTypeConfiguration : IEntityTypeConfiguration<EmployeeType>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EmployeeType> builder)
    {
        builder.ToTable(nameof(EmployeeType));
        
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.OwnsMany(e => e.Fields, fields =>
        {
            fields.ToJson("FieldsJson");
            
            fields.OwnsMany(f => f.Options);
        });
    }
}
