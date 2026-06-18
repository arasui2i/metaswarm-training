using CRM.Application.Features.Leads.CreateLead;
using CRM.Application.Interfaces;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Moq;

namespace CRM.Tests.Features.Leads;

[TestFixture]
public class CreateLeadHandlerTests
{
    private Mock<ILeadRepository> _repoMock = null!;
    private CreateLeadHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<ILeadRepository>();
        _handler = new CreateLeadHandler(_repoMock.Object);
    }

    [Test]
    public async Task Handle_ValidCommand_CallsAddAsyncAndReturnsGuid()
    {
        var expectedId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        var command = new CreateLeadCommand("Alice", null, "ACME Corp", "alice@acme.com", null, LeadStatus.New, null);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.EqualTo(expectedId));
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ValidCommand_MapsAllFieldsToLeadEntity()
    {
        Lead? capturedLead = null;
        var ownerId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .Callback<Lead, CancellationToken>((lead, _) => capturedLead = lead)
            .ReturnsAsync(Guid.NewGuid());

        var command = new CreateLeadCommand("Alice", "Smith", "ACME Corp", "alice@acme.com", "+1-555-0100", LeadStatus.Qualified, ownerId);
        await _handler.Handle(command, CancellationToken.None);

        Assert.That(capturedLead, Is.Not.Null);
        Assert.That(capturedLead!.FirstName, Is.EqualTo("Alice"));
        Assert.That(capturedLead.LastName, Is.EqualTo("Smith"));
        Assert.That(capturedLead.CompanyName, Is.EqualTo("ACME Corp"));
        Assert.That(capturedLead.Email, Is.EqualTo("alice@acme.com"));
        Assert.That(capturedLead.Phone, Is.EqualTo("+1-555-0100"));
        Assert.That(capturedLead.Status, Is.EqualTo(LeadStatus.Qualified));
        Assert.That(capturedLead.OwnerId, Is.EqualTo(ownerId));
    }

    [Test]
    public async Task Handle_ValidCommand_AssignsNewGuidAndCreatedAt()
    {
        Lead? capturedLead = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .Callback<Lead, CancellationToken>((lead, _) => capturedLead = lead)
            .ReturnsAsync(Guid.NewGuid());

        var before = DateTime.UtcNow;
        var command = new CreateLeadCommand("Alice", null, "ACME", "alice@acme.com", null, LeadStatus.New, null);
        await _handler.Handle(command, CancellationToken.None);
        var after = DateTime.UtcNow;

        Assert.That(capturedLead!.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(capturedLead.CreatedAt, Is.InRange(before, after));
    }

    [Test]
    public async Task Handle_WithNullOptionalFields_CreatesLeadWithNulls()
    {
        Lead? capturedLead = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .Callback<Lead, CancellationToken>((lead, _) => capturedLead = lead)
            .ReturnsAsync(Guid.NewGuid());

        var command = new CreateLeadCommand("Bob", null, "Beta", "bob@beta.com", null, LeadStatus.New, null);
        await _handler.Handle(command, CancellationToken.None);

        Assert.That(capturedLead!.LastName, Is.Null);
        Assert.That(capturedLead.Phone, Is.Null);
        Assert.That(capturedLead.OwnerId, Is.Null);
    }
}
