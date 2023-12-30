﻿using Microsoft.AspNetCore.Http;
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
        public IFormFile PassportImage { get; set; }
    }
}
