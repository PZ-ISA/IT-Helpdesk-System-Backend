using HelpdeskSystem.API.Extensions;
using HelpdeskSystem.Domain.Common;
using HelpdeskSystem.Domain.Dtos.Common;
using HelpdeskSystem.Domain.Dtos.Tickets;
using HelpdeskSystem.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HelpdeskSystem.API.Endpoints;

public static class TicketApi
{
    public static IEndpointRouteBuilder MapTicketApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tickets")
            .WithTags("Ticket")
            .RequireAuthorization("Employee")
            .RequireAuthorization("IsActive")
            .WithOpenApi();

        group.MapPost("", async (ITicketService ticketService, [FromBody] CreateTicketDto dto, CancellationToken ct) =>
        {
            var id = await ticketService.CreateTicketAsync(dto, ct);

            return Results.Created($"/api/tickets/{id}", null);
        })
        .WithRequestValidation<CreateTicketDto>()
        .Produces(StatusCodes.Status201Created);

        group.MapGet("", async (ITicketService ticketService, [AsParameters] PageQueryFilterDto filterDto, CancellationToken ct) =>
        {
            var result = await ticketService.GetTicketsAsync(filterDto, ct);

            return Results.Ok(result);
        })
        .WithRequestValidation<PageQueryFilterDto>()
        .Produces<PaginatedResponseDto<TicketDto>>(StatusCodes.Status200OK, "application/json");
        
        group.MapGet("/{id:guid}", async (ITicketService ticketService, Guid id, CancellationToken ct) =>
        {
            var result = await ticketService.GetTicketByIdAsync(id, ct);

            return Results.Ok(result);
        })
        .Produces<TicketDto>(StatusCodes.Status200OK, "application/json");
        
        group.MapPut("/{id:guid}", async (ITicketService ticketService, [FromBody] CreateTicketDto dto, Guid id, CancellationToken ct) =>
        {
            await ticketService.UpdateTicketAsync(dto, id, ct);

            return Results.NoContent();
        })
        .WithRequestValidation<CreateTicketDto>()
        .Produces(StatusCodes.Status204NoContent);
        
        group.MapDelete("/{id:guid}", async (ITicketService ticketService, Guid id, CancellationToken ct) =>
        {
            await ticketService.DeleteTicketAsync(id, ct);

            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent);
        
        group.MapPost("/{id:guid}/feedback", async (ITicketService ticketService, Guid id, [FromBody] FeedbackDto dto, CancellationToken ct) =>
        {
            await ticketService.AddFeedbackAsync(id, dto, ct);
            
            return Results.Ok();
         })
        .WithRequestValidation<FeedbackDto>()
        .Produces(StatusCodes.Status200OK);
        
        return app;
    }
}