using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace Data.Repositories
{
    public class TicketFileRepository : ITicketRepository
    {
        string _filePath;

        public TicketFileRepository(string filePath)
        {
            _filePath = filePath;

            if (!System.IO.File.Exists(filePath))
            {
                // Create the file and write an empty JSON array
                System.IO.File.WriteAllText(filePath, "[]");
            }
        }

        public void Book(Ticket ticket)
        {
            ticket.Id = Guid.NewGuid();

            string existingJson = System.IO.File.ReadAllText(_filePath);
            List<Ticket> Listicket = JsonSerializer.Deserialize<List<Ticket>>(existingJson);

            bool ExistingTicket = Listicket.Any(x => x.FlightIdFK == ticket.FlightIdFK && x.Row == ticket.Row && x.Column == ticket.Column && !x.Cancelled);

            if (!ExistingTicket)
            {
                Listicket.Add(ticket);

                string jsonString = JsonSerializer.Serialize(Listicket); //Converts from an object to a json string

                System.IO.File.WriteAllText(_filePath, jsonString);
            }
            else
            {
                throw new InvalidOperationException("The chosen seat is already booked. Please select another one.");
            }

        }

        public void Cancel(Guid ticketId)
        {
            var tickets = System.IO.File.ReadAllText(_filePath);
            
            if (tickets != null || tickets != "")
            {
                List<Ticket> ListTickets = JsonSerializer.Deserialize<List<Ticket>>(tickets);
                var ticket = ListTickets.FirstOrDefault(t => t.Id == ticketId);
                ticket.Cancelled = true;

                var jsonString = JsonSerializer.Serialize(ListTickets); //Converts from an object to a json string

                System.IO.File.WriteAllText(_filePath, jsonString);
            }
            
        }

        public Ticket GetTicket(Guid id)
        {
            var tickets = System.IO.File.ReadAllText(_filePath);

            if(tickets != null)
            {
                List<Ticket> ListTickets = JsonSerializer.Deserialize<List<Ticket>>(tickets);
                return ListTickets.SingleOrDefault(x => x.Id == id);
            }

            return new Ticket();
        }

        public IQueryable<Ticket> GetTickets(Guid flightId)
        {
            var tickets = System.IO.File.ReadAllText(_filePath);

            if (tickets != null)
            {
                List<Ticket> ListTickets = JsonSerializer.Deserialize<List<Ticket>>(tickets);
                return ListTickets.Where(t => t.FlightIdFK == flightId).AsQueryable();
            }
            
            return Enumerable.Empty<Ticket>().AsQueryable();
        }

        public IQueryable<Ticket> GetTicketsByUser(string passport)
        {
            var tickets = System.IO.File.ReadAllText(_filePath);

            if(tickets != null)
            {
                List<Ticket> ListTickets = JsonSerializer.Deserialize<List<Ticket>>(tickets);
                return ListTickets.Where(x => x.PassportNumber == passport).AsQueryable();
            }

            return Enumerable.Empty<Ticket>().AsQueryable();
        }
    }
}
