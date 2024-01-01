using Data.Repositories;
using Domain.Interfaces;
using Domain.Models;
using Domain.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class AdminController : Controller
    {
        private FlightDbRepository _flightDBRepository;

        private ITicketRepository _ticketDBRepository;

        private UserManager<AirlineUser> _userManager;

        public AdminController(FlightDbRepository flightDBRepository, ITicketRepository ticketDBRepository, UserManager<AirlineUser> userManager)
        {
            _flightDBRepository = flightDBRepository;
            _ticketDBRepository = ticketDBRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult SelectFlights()
        {
            if (User.Identity.IsAuthenticated == false)
            {
                TempData["error"] = "Access Denied"; //it survives redirection

                return RedirectToAction("Index", "Home"); //Redirecting the user to the view Index of the Home controller

            }
            var flights = _flightDBRepository.GetFlights().AsEnumerable()
                .Select(x => new FlightViewModel
                {
                    FlightId = x.Id,
                    CountryFrom = x.CountryFrom,
                    CountryTo = x.CountryTo,
                    ArrivalDate = x.ArrivalDate,
                    DepartureDate = x.DepartureDate,
                    AvailableSeats = GetAvailableSeatsCount(x),
                    RetailPrice = x.WholesalePrice * (x.WholesalePrice * x.CommissionRate) //calculating the retail price of the tickets
                }).ToList();
            return View(flights);
        }

        [HttpGet]
        public IActionResult SelectTickets(Guid flightid)
        {
            if (User.Identity.IsAuthenticated == false)
            {
                TempData["error"] = "Access Denied"; //it survives redirection

                return RedirectToAction("Index", "Home"); //Redirecting the user to the view Index of the Home controller

            }

            var tickets = _ticketDBRepository.GetTickets(flightid)
            .Select(ticket => new DetailTicketViewModel
            {
                Id = ticket.Id,
                FlightId = ticket.FlightIdFK,
                Row = ticket.Row,
                Column = ticket.Column,
                RetailPrice = ticket.PricePaid,
            }).AsEnumerable();

            return View(tickets);
        }

        [HttpGet]
        public IActionResult DetailsTicket(Guid TicketId)
        {
            if (User.Identity.IsAuthenticated == false)
            {
                TempData["error"] = "Access Denied"; //it survives redirection

                return RedirectToAction("Index", "Home"); //Redirecting the user to the view Index of the Home controller

            }
            var passport = "";

            var userId = _userManager.GetUserId(User);

            var user = _userManager.Users.FirstOrDefault(x => x.Id == userId);

            passport = user.PassportNumber;

            var ticket = _ticketDBRepository.GetTicket(TicketId);
            var view = new DetailTicketViewModel
            {
                Id = ticket.Id,
                FlightId = ticket.FlightIdFK,
                Row = ticket.Row,
                Column = ticket.Column,
                RetailPrice = ticket.PricePaid,
                PassportImagePath = ticket.PassportImagePath,
                Cancelled = ticket.Cancelled
            };

            return View(view);
        }

        [HttpGet]
        public IActionResult Cancel(Guid id)
        {
            var cancelTicket = _ticketDBRepository.GetTicket(id);

            //Properly check that the selected ticket to be cancelled exists and is not already cancelled
            if (cancelTicket != null && !cancelTicket.Cancelled)
            {
                _ticketDBRepository.Cancel(id);

            }

            return RedirectToAction("SelectFlights");
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
