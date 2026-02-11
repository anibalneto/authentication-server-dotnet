using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(al => al.Id);
        builder.Property(al => al.Id).ValueGeneratedOnAdd();

        builder.Property(al => al.Action).IsRequired().HasMaxLength(100);
        builder.Property(al => al.Resource).HasMaxLength(256);
        builder.Property(al => al.IpAddress).HasMaxLength(45);
        builder.Property(al => al.UserAgent).HasMaxLength(512);
        builder.Property(al => al.ErrorMessage).HasMaxLength(1024);

        builder.HasIndex(al => al.UserId);
        builder.HasIndex(al => al.CreatedAt);

        builder.HasOne(al => al.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
