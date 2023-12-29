using Data.Repositories;
using Domain.Models;
using Domain.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;

namespace Presentation.Controllers
{
    public class TicketsController : Controller
    {

        private TicketDBRepository _ticketDBRepository;

        private FlightDbRepository _flightDBRepository;

        private UserManager<AirlineUser> _userManager;
        
        public TicketsController(TicketDBRepository ticketDBRepository, FlightDbRepository flightDBRepository)
        {
            _ticketDBRepository = ticketDBRepository;
            _flightDBRepository = flightDBRepository;
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
                    RetailPrice = x.WholesalePrice * (x.WholesalePrice * x.CommissionRate) // calculating the retail price of the tickets
                }).ToList();

            return View(availableFlights);
        }

        [HttpGet]
        public IActionResult BookFlight(Guid flightId,float retailPrice)
        {
            TicketViewModel myModel = new TicketViewModel
            {
                FlightId = flightId,
                RetailPrice = retailPrice                
            };

            return View(myModel);
        }
        
        [HttpPost]
        public IActionResult BookFlight(TicketViewModel ticketViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(ticketViewModel);
            }

            var flight = _flightDBRepository.GetFlight(ticketViewModel.FlightId);

            if (flight == null ||  GetAvailableSeatsCount(flight) == 0 || flight.DepartureDate.ToUniversalTime() <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("Invalid flight selection.");
            }
            if (_ticketDBRepository.GetTickets(ticketViewModel.FlightId).Any(x =>  x.Row == ticketViewModel.Row && x.Column == ticketViewModel.Column && !x.Cancelled))
            {
                throw new InvalidOperationException("The chosen seat is already booked. Please select another one.");
            }

            ticketViewModel.RetailPrice = flight.WholesalePrice + (flight.WholesalePrice * flight.CommissionRate);

            //var userId = _userManager.GetUserId(User);
            
            _ticketDBRepository.Book(
                new Ticket()
                {
                    FlightIdFK = ticketViewModel.FlightId,
                    Column = ticketViewModel.Column,
                    Row = ticketViewModel.Row,
                    PricePaid = ticketViewModel.RetailPrice,
                    Cancelled = false,
                    PassportNumber = "not yet implemented", //#####not yet implemented
                    UserIdFk = 24//Convert.ToInt32(userId) //#####not yet implemented
                }
                );

            return View(ticketViewModel);
        }

        public IActionResult ShowTickets() //####REQUIRES TESTING
        {
            var userId = _userManager.GetUserId(User);
            var userGuid = new Guid(userId);
            var tickets = _ticketDBRepository.GetTickets(userGuid);
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
