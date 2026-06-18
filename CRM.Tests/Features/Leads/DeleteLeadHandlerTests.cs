using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Leads.DeleteLead;
using CRM.Application.Interfaces;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Moq;

namespace CRM.Tests.Features.Leads;

[TestFixture]
public class DeleteLeadHandlerTests
{
    private Mock<ILeadRepository> _repoMock = null!;
    private DeleteLeadHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<ILeadRepository>();
        _handler = new DeleteLeadHandler(_repoMock.Object);
    }

    [Test]
    public async Task Handle_ExistingLead_CallsSoftDeleteAsync()
    {
        var id = Guid.NewGuid();
        var lead = new Lead { Id = id, FirstName = "Alice", CompanyName = "ACME", Email = "a@a.com", Status = LeadStatus.New, CreatedAt = DateTime.UtcNow };
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _repoMock.Setup(r => r.SoftDeleteAsync(id, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _handler.Handle(new DeleteLeadCommand(id), CancellationToken.None);

        _repoMock.Verify(r => r.SoftDeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Handle_LeadNotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Lead?)null);

        Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(new DeleteLeadCommand(id), CancellationToken.None));
    }

    [Test]
    public async Task Handle_ExistingLead_DoesNotCallSoftDeleteWhenGetByIdReturnsNull()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Lead?)null);

        try { await _handler.Handle(new DeleteLeadCommand(id), CancellationToken.None); }
        catch (NotFoundException) { }

        _repoMock.Verify(r => r.SoftDeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
