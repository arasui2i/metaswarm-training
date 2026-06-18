using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Leads;
using CRM.Application.Features.Leads.GetLeadById;
using CRM.Application.Interfaces;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Moq;

namespace CRM.Tests.Features.Leads;

[TestFixture]
public class GetLeadByIdHandlerTests
{
    private Mock<ILeadRepository> _repoMock = null!;
    private GetLeadByIdHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<ILeadRepository>();
        _handler = new GetLeadByIdHandler(_repoMock.Object);
    }

    [Test]
    public async Task Handle_ExistingLead_ReturnsMappedDetailDto()
    {
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var lead = new Lead
        {
            Id = id,
            FirstName = "Alice",
            LastName = "Smith",
            CompanyName = "ACME Corp",
            Email = "alice@acme.com",
            Phone = "+1-555-0100",
            Status = LeadStatus.Qualified,
            OwnerId = ownerId,
            CreatedAt = createdAt
        };
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);

        var result = await _handler.Handle(new GetLeadByIdQuery(id), CancellationToken.None);

        Assert.That(result.Id, Is.EqualTo(id));
        Assert.That(result.FirstName, Is.EqualTo("Alice"));
        Assert.That(result.LastName, Is.EqualTo("Smith"));
        Assert.That(result.CompanyName, Is.EqualTo("ACME Corp"));
        Assert.That(result.Email, Is.EqualTo("alice@acme.com"));
        Assert.That(result.Phone, Is.EqualTo("+1-555-0100"));
        Assert.That(result.Status, Is.EqualTo(LeadStatus.Qualified));
        Assert.That(result.OwnerId, Is.EqualTo(ownerId));
        Assert.That(result.CreatedAt, Is.EqualTo(createdAt));
    }

    [Test]
    public void Handle_LeadNotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Lead?)null);

        Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(new GetLeadByIdQuery(id), CancellationToken.None));
    }

    [Test]
    public async Task Handle_LeadWithNullOptionalFields_MapsNullsToDto()
    {
        var id = Guid.NewGuid();
        var lead = new Lead { Id = id, FirstName = "Bob", LastName = null, CompanyName = "Beta", Email = "bob@beta.com", Phone = null, Status = LeadStatus.New, OwnerId = null, CreatedAt = DateTime.UtcNow };
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);

        var result = await _handler.Handle(new GetLeadByIdQuery(id), CancellationToken.None);

        Assert.That(result.LastName, Is.Null);
        Assert.That(result.Phone, Is.Null);
        Assert.That(result.OwnerId, Is.Null);
    }
}
