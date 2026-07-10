using Dynamic.Employees.Domain.Models;
using Dynamic.Json.EfCore.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dynamic.Employees.Data.Configurations;

/// <summary>
/// Configures persistence for employee entities.
/// </summary>
/// <remarks>
/// Runtime-defined values are stored in one JSON column and configured through Dynamic.Json so
/// provider-specific search expressions can be translated to SQL Server rather than evaluated in memory.
/// </remarks>
public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    private const string FieldValuesColumnName = "FieldValuesJson";

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable(nameof(Employee));
        
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Department)
            .HasMaxLength(100);

        builder.HasOne(e => e.EmployeeType)
            .WithMany()
            .HasForeignKey(e => e.EmployeeTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.FirstName);
        builder.HasIndex(e => e.LastName);
        builder.HasIndex(e => e.EmployeeTypeId);

        builder.Property(e => e.FieldValues)
            .HasColumnName(FieldValuesColumnName)
            .HasJsonConversion();
    }
}
