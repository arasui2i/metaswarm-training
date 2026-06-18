using CRM.Application.Common;
using CRM.Application.Features.Leads;
using CRM.Application.Features.Leads.GetLeads;
using CRM.Application.Interfaces;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Moq;

namespace CRM.Tests.Features.Leads;

[TestFixture]
public class GetLeadsHandlerTests
{
    private Mock<ILeadRepository> _repoMock = null!;
    private GetLeadsHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<ILeadRepository>();
        _handler = new GetLeadsHandler(_repoMock.Object);
    }

    private static Lead MakeLead(string firstName, string? lastName, string company, string email, LeadStatus status = LeadStatus.New) =>
        new()
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            CompanyName = company,
            Email = email,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

    [Test]
    public async Task Handle_ReturnsMappedPagedResult()
    {
        var leads = new List<Lead>
        {
            MakeLead("Alice", "Smith", "ACME Corp", "alice@acme.com", LeadStatus.Qualified),
            MakeLead("Bob", "Jones", "Beta Ltd", "bob@beta.com", LeadStatus.Contacted),
        };
        _repoMock
            .Setup(r => r.GetPagedAsync(null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Lead>(leads, 2, 1, 10));

        var query = new GetLeadsQuery(null, 1, 10);
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(2));
        Assert.That(result.Page, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(10));
        Assert.That(result.Items.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Handle_MapsFullNameCorrectly()
    {
        var leads = new List<Lead> { MakeLead("Alice", "Smith", "ACME", "alice@acme.com") };
        _repoMock
            .Setup(r => r.GetPagedAsync(null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Lead>(leads, 1, 1, 10));

        var result = await _handler.Handle(new GetLeadsQuery(null, 1, 10), CancellationToken.None);

        Assert.That(result.Items[0].FullName, Is.EqualTo("Alice Smith"));
    }

    [Test]
    public async Task Handle_NullLastName_FullNameIsTrimmedFirstName()
    {
        var leads = new List<Lead> { MakeLead("Bob", null, "Beta", "bob@beta.com") };
        _repoMock
            .Setup(r => r.GetPagedAsync(null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Lead>(leads, 1, 1, 10));

        var result = await _handler.Handle(new GetLeadsQuery(null, 1, 10), CancellationToken.None);

        Assert.That(result.Items[0].FullName, Is.EqualTo("Bob"));
    }

    [Test]
    public async Task Handle_MapsStatusCorrectly()
    {
        var leads = new List<Lead> { MakeLead("Alice", null, "ACME", "a@acme.com", LeadStatus.Converted) };
        _repoMock
            .Setup(r => r.GetPagedAsync(null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Lead>(leads, 1, 1, 10));

        var result = await _handler.Handle(new GetLeadsQuery(null, 1, 10), CancellationToken.None);

        Assert.That(result.Items[0].Status, Is.EqualTo(LeadStatus.Converted));
    }

    [Test]
    public async Task Handle_PassesSearchTermToRepository()
    {
        _repoMock
            .Setup(r => r.GetPagedAsync("alice", 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Lead>(new List<Lead>(), 0, 1, 10));

        var query = new GetLeadsQuery("alice", 1, 10);
        await _handler.Handle(query, CancellationToken.None);

        _repoMock.Verify(r => r.GetPagedAsync("alice", 1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_PassesPagingParametersToRepository()
    {
        _repoMock
            .Setup(r => r.GetPagedAsync(null, 3, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Lead>(new List<Lead>(), 0, 3, 5));

        var query = new GetLeadsQuery(null, 3, 5);
        var result = await _handler.Handle(query, CancellationToken.None);

        _repoMock.Verify(r => r.GetPagedAsync(null, 3, 5, It.IsAny<CancellationToken>()), Times.Once);
        Assert.That(result.Page, Is.EqualTo(3));
        Assert.That(result.PageSize, Is.EqualTo(5));
    }

    [Test]
    public async Task Handle_EmptyResult_ReturnsEmptyItems()
    {
        _repoMock
            .Setup(r => r.GetPagedAsync(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Lead>(new List<Lead>(), 0, 1, 10));

        var result = await _handler.Handle(new GetLeadsQuery(null, 1, 10), CancellationToken.None);

        Assert.That(result.Items, Is.Empty);
        Assert.That(result.TotalCount, Is.EqualTo(0));
    }

    [Test]
    public async Task Handle_MapsDtoFieldsFromLeadEntity()
    {
        var lead = MakeLead("Carol", "White", "Gamma Inc", "carol@gamma.com", LeadStatus.Unqualified);
        _repoMock
            .Setup(r => r.GetPagedAsync(null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Lead>(new List<Lead> { lead }, 1, 1, 10));

        var result = await _handler.Handle(new GetLeadsQuery(null, 1, 10), CancellationToken.None);

        var dto = result.Items[0];
        Assert.That(dto.Id, Is.EqualTo(lead.Id));
        Assert.That(dto.CompanyName, Is.EqualTo("Gamma Inc"));
        Assert.That(dto.Email, Is.EqualTo("carol@gamma.com"));
        Assert.That(dto.Status, Is.EqualTo(LeadStatus.Unqualified));
    }
}
