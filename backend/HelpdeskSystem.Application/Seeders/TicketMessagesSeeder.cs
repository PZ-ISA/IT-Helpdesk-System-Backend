using Bogus;
using HelpdeskSystem.Domain.Entities;
using HelpdeskSystem.Domain.Enums;
using HelpdeskSystem.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskSystem.Application.Seeders;

public static class TicketMessagesSeeder
{
    private static readonly Random R = new();
    
    public static async Task SeedAsync(HelpdeskDbContext dbContext)
    {
        if (await dbContext.TicketMessages.AnyAsync())
        {
            return;
        }

        var sampleTickets = await dbContext.Tickets
            .Where(t => t.Status != TicketStatus.New && t.AdminUserId != null)
            .ToListAsync();
        
        if (sampleTickets.Count == 0)
        {
            return;
        }

        var faker = new Faker<TicketMessage>("pl")
            .RuleFor(t => t.Message, f => f.Lorem.Sentences());
        
        var ticketMessages = new List<TicketMessage>();
        
        foreach (var ticket in sampleTickets)
        {
            // Generate 1-5 messages per ticket
            var ticketMessagesCount = R.Next(3, 8);
            for (var i = 0; i < ticketMessagesCount; i++)
            {
                // Chose who is author of the message
                var isAdminMessage = R.NextDouble() < 0.5;

                var message = faker.Generate();
                message.TicketId = ticket.Id;
                message.UserId = isAdminMessage ? ticket.AdminUserId!.Value : ticket.EmployeeUserId;
                
                ticketMessages.Add(message);
            }
        }
        
        await dbContext.TicketMessages.AddRangeAsync(ticketMessages);
        await dbContext.SaveChangesAsync();
    }
}