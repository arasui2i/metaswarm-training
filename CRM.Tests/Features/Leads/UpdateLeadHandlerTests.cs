using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Leads.UpdateLead;
using CRM.Application.Interfaces;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Moq;

namespace CRM.Tests.Features.Leads;

[TestFixture]
public class UpdateLeadHandlerTests
{
    private Mock<ILeadRepository> _repoMock = null!;
    private UpdateLeadHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<ILeadRepository>();
        _handler = new UpdateLeadHandler(_repoMock.Object);
    }

    private Lead ExistingLead(Guid id) => new()
    {
        Id = id,
        FirstName = "Old",
        LastName = "Name",
        CompanyName = "Old Corp",
        Email = "old@corp.com",
        Status = LeadStatus.New,
        CreatedAt = DateTime.UtcNow
    };

    [Test]
    public async Task Handle_ExistingLead_UpdatesFieldsAndCallsUpdateAsync()
    {
        var id = Guid.NewGuid();
        var existing = ExistingLead(id);
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var cmd = new UpdateLeadCommand(id, "Alice", "Smith", "ACME", "alice@acme.com", "+1-555", LeadStatus.Qualified, null);
        await _handler.Handle(cmd, CancellationToken.None);

        Assert.That(existing.FirstName, Is.EqualTo("Alice"));
        Assert.That(existing.LastName, Is.EqualTo("Smith"));
        Assert.That(existing.CompanyName, Is.EqualTo("ACME"));
        Assert.That(existing.Email, Is.EqualTo("alice@acme.com"));
        Assert.That(existing.Phone, Is.EqualTo("+1-555"));
        Assert.That(existing.Status, Is.EqualTo(LeadStatus.Qualified));
        _repoMock.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Handle_LeadNotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Lead?)null);

        var cmd = new UpdateLeadCommand(id, "Alice", null, "ACME", "alice@acme.com", null, LeadStatus.New, null);

        Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(cmd, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ExistingLead_UpdatesOwnerIdToNull()
    {
        var id = Guid.NewGuid();
        var existing = ExistingLead(id);
        existing.OwnerId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var cmd = new UpdateLeadCommand(id, "Alice", null, "ACME", "alice@acme.com", null, LeadStatus.New, null);
        await _handler.Handle(cmd, CancellationToken.None);

        Assert.That(existing.OwnerId, Is.Null);
    }
}
