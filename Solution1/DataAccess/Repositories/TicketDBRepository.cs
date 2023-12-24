using Data.Context;
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

        //Book()

        //Cancel()

        //GetTickets()
    }
}
