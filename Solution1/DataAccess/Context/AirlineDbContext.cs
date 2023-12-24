using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Context
{
    public class AirlineDbContext : DbContext
    {

        public AirlineDbContext(DbContextOptions<AirlineDbContext> option): base(option)
        {

        }

        public DbSet<Flight> Flights { get; set; } //MUST BEEN IN PLURAL table names in plural, models in singular
        public DbSet<Ticket> Tickets { get; set; }
    }
}
