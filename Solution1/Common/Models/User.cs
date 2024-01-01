using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class AirlineUser : IdentityUser
    {
        public string? PassportNumber { get; set; }

    }
}
