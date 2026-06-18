using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Leads.CreateLead;
using CRM.Application.Features.Leads.DeleteLead;
using CRM.Application.Features.Leads.GetLeadById;
using CRM.Application.Features.Leads.GetLeads;
using CRM.Application.Features.Leads.UpdateLead;
using CRM.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeadsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeadsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "leads.view")]
    public async Task<IActionResult> GetLeads([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetLeadsQuery(search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "leads.view")]
    public async Task<IActionResult> GetLeadById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetLeadByIdQuery(id), cancellationToken);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "leads.create")]
    public async Task<IActionResult> CreateLead([FromBody] CreateLeadRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(
            new CreateLeadCommand(request.FirstName, request.LastName, request.CompanyName, request.Email, request.Phone, request.Status ?? LeadStatus.New, request.OwnerId),
            cancellationToken);
        return CreatedAtAction(nameof(GetLeadById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "leads.edit")]
    public async Task<IActionResult> UpdateLead(Guid id, [FromBody] UpdateLeadRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(
                new UpdateLeadCommand(id, request.FirstName, request.LastName, request.CompanyName, request.Email, request.Phone, request.Status ?? LeadStatus.New, request.OwnerId),
                cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "leads.delete")]
    public async Task<IActionResult> DeleteLead(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new DeleteLeadCommand(id), cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

public record CreateLeadRequest(string FirstName, string? LastName, string CompanyName, string Email, string? Phone, LeadStatus? Status, Guid? OwnerId);
public record UpdateLeadRequest(string FirstName, string? LastName, string CompanyName, string Email, string? Phone, LeadStatus? Status, Guid? OwnerId);
