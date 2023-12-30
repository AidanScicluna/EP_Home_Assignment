using Data.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public class TicketDBRepository
    {
        AirlineDbContext _context;

        public TicketDBRepository(AirlineDbContext context)
        {
            _context = context;
        }

        // checks that the selected ticket is valid (not already in use / cancelled) and saves it to the database
        public void Book(Ticket ticket)
        {
            if (_context.Tickets.Any(x => x.FlightIdFK == ticket.FlightIdFK && x.Row == ticket.Row && x.Column == ticket.Column && !x.Cancelled))
            {
                throw new InvalidOperationException("The chosen seat is already booked. Please select another one.");
            }

            _context.Tickets.Add(ticket);
            _context.SaveChanges();

        }

        //the booked ticket will be cancelled but not deleted (the seat can be used/selected again by someone else)
        public void Cancel(Guid id)
        {
            var cancelTicket = _context.Tickets.FirstOrDefault(x => x.Id == id);

            //Properly check that the selected ticket to be cancelled exists and is not already cancelled
            if (cancelTicket != null && !cancelTicket.Cancelled)
            {
                cancelTicket.Cancelled = true;
                _context.SaveChanges();
            }
        }

        //Returns all the tickets for a flight selected
        public IQueryable<Ticket> GetTickets(Guid id)
        {
            return _context.Tickets.Where(x => x.FlightIdFK == id);
        }

        //Returns all the tickets that were bought by a user 
        public IQueryable<Ticket> GetTicketsByUser(string passport)
        {
            return _context.Tickets.Where(x => x.PassportNumber == passport);
        }

        public Ticket GetTicket(Guid id)
        {
            return _context.Tickets.SingleOrDefault(x => x.Id == id);
        }
    }
}
