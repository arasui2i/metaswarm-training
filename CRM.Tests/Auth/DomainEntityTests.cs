using CRM.Domain.Entities;

namespace CRM.Tests.Auth;

[TestFixture]
public class DomainEntityTests
{
    [Test]
    public void Permission_Properties_SetCorrectly()
    {
        var id = Guid.NewGuid();
        var permission = new Permission
        {
            Id = id,
            Name = "View Customers",
            ActionKey = "customers.view"
        };

        Assert.That(permission.Id, Is.EqualTo(id));
        Assert.That(permission.Name, Is.EqualTo("View Customers"));
        Assert.That(permission.ActionKey, Is.EqualTo("customers.view"));
        Assert.That(permission.RolePermissions, Is.Empty);
    }

    [Test]
    public void RolePermission_Properties_SetCorrectly()
    {
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        var role = new Role { Id = roleId, Name = "Admin" };
        var permission = new Permission { Id = permissionId, Name = "Edit", ActionKey = "customers.edit" };

        var rp = new RolePermission
        {
            RoleId = roleId,
            Role = role,
            PermissionId = permissionId,
            Permission = permission
        };

        Assert.That(rp.RoleId, Is.EqualTo(roleId));
        Assert.That(rp.PermissionId, Is.EqualTo(permissionId));
        Assert.That(rp.Role.Name, Is.EqualTo("Admin"));
        Assert.That(rp.Permission.ActionKey, Is.EqualTo("customers.edit"));
    }
}
