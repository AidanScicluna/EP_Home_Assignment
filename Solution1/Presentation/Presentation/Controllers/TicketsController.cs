using Data.Repositories;
using Domain.Interfaces;
using Domain.Models;
using Domain.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Net.Sockets;

namespace Presentation.Controllers
{
    public class TicketsController : Controller
    {

        private ITicketRepository _ticketDBRepository;

        private FlightDbRepository _flightDBRepository;

        private UserManager<AirlineUser> _userManager;
        
        public TicketsController(ITicketRepository ticketDBRepository, FlightDbRepository flightDBRepository, UserManager<AirlineUser> userManager)
        {
            _ticketDBRepository = ticketDBRepository;
            _flightDBRepository = flightDBRepository;
            _userManager = userManager;
        }

        //shows the user all the available flights (i.e not fullied booked and/or in the past)
        //note: pilots and the ATC use UTC so when calculating departure and arrival, they should be changed to UTC (the flight's departure and
        //arival are in terms of the local times at the respective airports) source: ICAO SARPs Annex 2 (Rules of the Air)
        public IActionResult showAvailableFlights()
        {
            var availableFlights = _flightDBRepository.GetFlights().AsEnumerable().
                Where(x => GetAvailableSeatsCount(x) > 0 && x.DepartureDate.ToUniversalTime() > DateTime.UtcNow)
                .Select(x => new FlightViewModel
                {
                    FlightId = x.Id,
                    CountryFrom = x.CountryFrom,
                    CountryTo = x.CountryTo,
                    ArrivalDate = x.ArrivalDate,
                    DepartureDate = x.DepartureDate,
                    RetailPrice = x.WholesalePrice * (x.WholesalePrice * x.CommissionRate) //calculating the retail price of the tickets
                }).ToList();

            return View(availableFlights);
        }

        [HttpGet]
        public IActionResult BookFlight(Guid flightId, float retailPrice)
        {
            TicketViewModel myModel = new TicketViewModel
            {
                FlightId = flightId,
                RetailPrice = retailPrice                
            };

            return View(myModel);
        }
        
        [HttpPost]
        public IActionResult BookFlight(TicketViewModel ticketViewModel ,[FromServices] IWebHostEnvironment host)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            var errorMessage = error.ErrorMessage;
                            var exception = error.Exception;
                            
                            // Log or print the error details for debugging
                        }
                    }
                    return View(ticketViewModel);
                }

                var flight = _flightDBRepository.GetFlight(ticketViewModel.FlightId);

                if (flight == null || GetAvailableSeatsCount(flight) == 0 || flight.DepartureDate.ToUniversalTime() <= DateTime.UtcNow)
                {
                    throw new InvalidOperationException("Invalid flight selection.");
                }
                if (_ticketDBRepository.GetTickets(ticketViewModel.FlightId).Any(x => x.Row == ticketViewModel.Row && x.Column == ticketViewModel.Column && !x.Cancelled))
                {
                    throw new InvalidOperationException("The chosen seat is already booked. Please select another one.");
                }

                string relativePath = "";

                string passport = "";

                //upload of an image
                if (ticketViewModel.PassportImage != null)
                {
                    //1. generate a unique filename
                    string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(ticketViewModel.PassportImage.FileName); //48FF33F2-DE10-42CE-8B55-FCA4F4F35C52.jpg

                    //2. form the relative path
                    relativePath = "/images/" + newFileName;

                    //3. form the absoulte path to save the physical file
                    string absolutePath = host.WebRootPath + "\\images\\" + newFileName; //if this is wrong it won't save your images
                                                                                         //(the name images is from the folder in wwwroot so if you name it somethign different change it to that)

                    //4. save the image in the folder
                    using (FileStream fs = new FileStream(absolutePath, FileMode.CreateNew))
                    {
                        ticketViewModel.PassportImage.CopyTo(fs);
                        fs.Flush();
                    }

                }

                ticketViewModel.RetailPrice = flight.WholesalePrice + (flight.WholesalePrice * flight.CommissionRate);

                var userId = _userManager.GetUserId(User);

                var user = _userManager.Users.FirstOrDefault(x => x.Id == userId);

                passport = user.PassportNumber;

                _ticketDBRepository.Book(
                    new Ticket()
                    {
                        FlightIdFK = ticketViewModel.FlightId,
                        Column = ticketViewModel.Column,
                        Row = ticketViewModel.Row,
                        PricePaid = ticketViewModel.RetailPrice,
                        Cancelled = false,
                        PassportImagePath = relativePath,
                        PassportNumber = passport
                    }
                    );

                return View(ticketViewModel);
            }catch(Exception ex)
            {
                TempData["error"] = "an error occured";
                return View(ticketViewModel);
            }
        }

        public IActionResult ShowTickets()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var userId = _userManager.GetUserId(User);
            
            var user = _userManager.Users.FirstOrDefault(x => x.Id == userId);

            string passport = user.PassportNumber;

            var tickets = _ticketDBRepository.GetTicketsByUser(passport)
            .Select(ticket => new TicketViewModel
            {
                FlightId = ticket.FlightIdFK,
                Row = ticket.Row,
                Column = ticket.Column,
                RetailPrice = ticket.PricePaid
            }).ToList();
            return View(tickets);
        }

        //gets all the available seats of any selected flight
        //note: the average commercial airplane has around 150 to 300 seats, the average flight has around 100 passangers
        public int GetAvailableSeatsCount(Flight flight)
        {
            var totalSeats = flight.Rows * flight.Columns;
            var bookedSeatsCount = _ticketDBRepository.GetTickets(flight.Id).Count(x => x.FlightIdFK == flight.Id && !x.Cancelled);
            var availableSeatsCount = totalSeats - bookedSeatsCount;

            return availableSeatsCount;
        }

    }
}
