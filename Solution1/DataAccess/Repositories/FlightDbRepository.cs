using Data.Context;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public class FlightDbRepository
    {
        AirlineDbContext _context;

        public FlightDbRepository(AirlineDbContext context)
        {
            _context = context;
        }

        public IQueryable<Flight> GetFlights() //EFFICIENTLY BUILT MEANS USING IQUERYABLE INSTEAD OF LIST
        {
            return _context.Flights;
        }

        public Flight? GetFlight(Guid id) //only get a single flight as oposied to a list such as getFlights
        {
            return GetFlights().SingleOrDefault(x => x.Id == id);
        }
    }
}
