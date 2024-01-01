using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models.ViewModels
{
    public class DetailTicketViewModel
    {
        [Key]
        public Guid Id { get; set; }
        public Guid FlightId { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public float RetailPrice { get; set; }
        public string? PassportImagePath { get; set; }
        public bool Cancelled { get; set; }
    }
}
