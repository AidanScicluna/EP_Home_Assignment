using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models.ViewModels
{
    public class TicketViewModel
    {
        public Guid FlightId { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public float RetailPrice { get; set; }
        public string PassportNumber { get; set; }
        public IFormFile PassportImage { get; set; }
        public int MaxRow { get; set; }
        public int MaxColumn { get; set; }

        [Remote(action: "IsSeatAvailable", controller: "TicketsController", ErrorMessage = "This seat is already booked.")]
        public string SelectedSeat { get; set; }
    }
}
