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

        //OnModelCreating will make sure that for every new flight/ticket that i create (even) in the database ...a guid will be automatically created
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Flight>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<Ticket>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
        }
    }
}
