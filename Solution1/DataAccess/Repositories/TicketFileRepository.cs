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

            if (System.IO.File.Exists(filePath) == false)
            {
                using (var myFile = System.IO.File.Create(filePath))
                {
                    myFile.Close(); //if you dont close yourselves it will give us an error later up...
                }
            }
        }

        public void Book(Ticket ticket)
        {
            ticket.Id = Guid.NewGuid();

            var tickets = GetTickets(ticket.FlightIdFK).Where(ticket => !ticket.Cancelled).ToList();
            tickets.Add(ticket);

            string jsonString = JsonSerializer.Serialize(tickets); //Converts from an object to a json string

            System.IO.File.WriteAllText(_filePath, jsonString);
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
