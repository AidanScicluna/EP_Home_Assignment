using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Ticket
    {
        [Key]
        public Guid Id { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public Flight FlightIdFK { get; set; }
        public string Passport { get; set; }
        public float PricePaid { get; set; }
        public bool Cancelled { get; set; }
    }
}
