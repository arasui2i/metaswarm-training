using System.Diagnostics.CodeAnalysis;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence;

[ExcludeFromCodeCoverage]
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Lead> Leads => Set<Lead>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
        modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });

        var adminRoleId = Guid.Parse("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1");
        var salesRoleId = Guid.Parse("b2b2b2b2-b2b2-b2b2-b2b2-b2b2b2b2b2b2");
        var viewerRoleId = Guid.Parse("c3c3c3c3-c3c3-c3c3-c3c3-c3c3c3c3c3c3");

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = adminRoleId, Name = "Admin" },
            new Role { Id = salesRoleId, Name = "Sales" },
            new Role { Id = viewerRoleId, Name = "Viewer" }
        );

        var customersViewId = Guid.Parse("d4d4d4d4-d4d4-d4d4-d4d4-d4d4d4d4d4d4");
        var customersEditId = Guid.Parse("e5e5e5e5-e5e5-e5e5-e5e5-e5e5e5e5e5e5");
        var leadsViewId = Guid.Parse("f6f6f6f6-f6f6-f6f6-f6f6-f6f6f6f6f6f6");
        var leadsCreateId = Guid.Parse("a7a7a7a7-a7a7-a7a7-a7a7-a7a7a7a7a7a7");
        var leadsEditId = Guid.Parse("b8b8b8b8-b8b8-b8b8-b8b8-b8b8b8b8b8b8");
        var leadsDeleteId = Guid.Parse("c9c9c9c9-c9c9-c9c9-c9c9-c9c9c9c9c9c9");

        modelBuilder.Entity<Permission>().HasData(
            new Permission { Id = customersViewId, Name = "Customers View", ActionKey = "customers.view" },
            new Permission { Id = customersEditId, Name = "Customers Edit", ActionKey = "customers.edit" },
            new Permission { Id = leadsViewId, Name = "Leads View", ActionKey = "leads.view" },
            new Permission { Id = leadsCreateId, Name = "Leads Create", ActionKey = "leads.create" },
            new Permission { Id = leadsEditId, Name = "Leads Edit", ActionKey = "leads.edit" },
            new Permission { Id = leadsDeleteId, Name = "Leads Delete", ActionKey = "leads.delete" }
        );

        modelBuilder.Entity<RolePermission>().HasData(
            // Admin — all permissions
            new RolePermission { RoleId = adminRoleId, PermissionId = customersViewId },
            new RolePermission { RoleId = adminRoleId, PermissionId = customersEditId },
            new RolePermission { RoleId = adminRoleId, PermissionId = leadsViewId },
            new RolePermission { RoleId = adminRoleId, PermissionId = leadsCreateId },
            new RolePermission { RoleId = adminRoleId, PermissionId = leadsEditId },
            new RolePermission { RoleId = adminRoleId, PermissionId = leadsDeleteId },
            // Sales — leads CRUD + customers view
            new RolePermission { RoleId = salesRoleId, PermissionId = customersViewId },
            new RolePermission { RoleId = salesRoleId, PermissionId = leadsViewId },
            new RolePermission { RoleId = salesRoleId, PermissionId = leadsCreateId },
            new RolePermission { RoleId = salesRoleId, PermissionId = leadsEditId },
            // Viewer — view only
            new RolePermission { RoleId = viewerRoleId, PermissionId = customersViewId },
            new RolePermission { RoleId = viewerRoleId, PermissionId = leadsViewId }
        );

        modelBuilder.Entity<Lead>(entity =>
        {
            entity.ToTable("Leads");
            entity.Property(l => l.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(l => l.CompanyName).IsRequired().HasMaxLength(200);
            entity.Property(l => l.Email).IsRequired().HasMaxLength(200);
            entity.Property(l => l.Status).HasDefaultValue(LeadStatus.New);
        });
    }
}
